using System;
using System.Linq;
using System.Globalization;

namespace worldgenerator
{
    public enum Biome
    {
        Undetermined = 0,
        Ice = 1,
        Tundra = 2,
        Taiga = 3,
        TemperateForest = 4,
        Grassland = 5,
        Desert = 6,
        TropicalRainforest = 7,
        Savanna = 8,
        Marine = 9,
        Mountain = 10
    }

    public class WorldGen
    {
        public static int DefaultSize { get { return 200; } }
        public string SeedInfo
        {
            get
            {
                string cw = "", tw = "", ww = "", si;
                foreach (float weight in ColdWeights) {
                    cw += $" { weight }";
                }

                foreach (float weight in TemparateWeights) {
                    tw += $" { weight }";
                }

                foreach (float weight in WarmWeights) {
                    ww  += $" { weight }";
                }

                si = $"Seed: { seed.ToString("X4") } Size: { size } Stickiness: { stickiness }";

                return $"Weights:\n{ cw }\n{ tw }\n{ ww }\n{ si }";
            }
        }

        // Weights determine the chance of each Biome appearing in each Temperature zone. Index in the array corresponds to a Biome in the Biome enum
        public float[] ColdWeights { get { return new float[] { 0.0f, 10.0f, 11.0f, 11.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 55.0f, 4.0f }; } }
        public float[] TemparateWeights { get { return new float[] { 0.0f, 0f, 0f, 0f, 10.0f, 10.0f, 0f, 0f, 0f, 53.0f, 4.0f }; } }
        public float[] WarmWeights { get { return new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0f, 0f, 12.0f, 12.0f, 12.0f, 55.0f, 4.0f }; } }

        public Biome[,] BiomeGrid { get { return biomeGrid; } }
        public int size;
        public int seed;
        public float stickiness = 40.0f;

        private Random random;
        private Biome[,] biomeGrid;
        

        private enum TemperatureZone
        {
            Warm = 0,
            Temparate = 1,
            Cold = 2
        }

        /// <summary>Readies world generation with a randomly generated seed</summary>
        public WorldGen()
        {
            GenerateSeed();
            size = DefaultSize;
        }

        /// <summary>Readies world generation with a randomly generated seed and custom size</summary>
        /// <param name="Size">an int to determine how many rows/columns the world should be</param>
        public WorldGen(int Size)
        {
            GenerateSeed();
            size = Size;
        }

        /// <summary>Readies world generation with a predetermined seed</summary>
        /// <param name="predeterminedSeed">Accepts a string that is a valid Hex String</param>
        /// <param name="Size">an int to determine how many rows/columns the world should be</param>
        public WorldGen(string predeterminedSeed, int? Size)
        {
            // strip 0x if it exists, TryParse doesn't like it
            if (predeterminedSeed.StartsWith("0x"))
            {
                predeterminedSeed = predeterminedSeed.Substring(2);
            }

            bool success = int.TryParse(predeterminedSeed,
                                        System.Globalization.NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture,
                                        out seed);

            if (!success)
            {
                throw new FormatException("predeterminedSeed must be a valid hex string");
            }

            size = Size.HasValue ? Size.Value : DefaultSize;
        }

        /// <summary>Gets a random int and assigns it to seed</summary>
        private void GenerateSeed()
        {
            System.Random rnd = new Random(); // specify System.Random for Unity
            seed = rnd.Next(Int32.MaxValue);
        }

        public void GenerateBiomes()
        {
            random = new Random(seed); // start the RNG over
            biomeGrid = new Biome[size, size];
            int midPoint = size / 2;

            //int count = 0;

            foreach (int row in Enumerable.Range(0, size).OrderBy(x => random.Next()))
            {  // random iteration prevents right-to-left striation
                foreach (int col in Enumerable.Range(0, size).OrderBy(x => random.Next()))
                {
                    // find neighbors of current biome
                    Biome[] neighbors = new Biome[4];

                    // find north neighbor coords
                    if (row == 0)
                    { // it's a globe, find the north neighbor of the northmost
                        int ncol;
                        if (col >= midPoint)
                        {
                            ncol = col - midPoint;
                        }
                        else
                        {
                            ncol = col + midPoint;
                        }
                        neighbors[0] = biomeGrid[0, ncol];
                    }
                    else
                    {
                        neighbors[0] = biomeGrid[row - 1, col];
                    }

                    // find east neighbor coords
                    if (col == size - 1)
                    {
                        neighbors[1] = biomeGrid[row, 0]; // wrap around the world
                    }
                    else
                    {
                        neighbors[1] = biomeGrid[row, col + 1];
                    }

                    // find south neighbor coords
                    if (row == size - 1)
                    {
                        int scol;
                        if (col >= midPoint)
                        {
                            scol = col - midPoint;
                        }
                        else
                        {
                            scol = col + midPoint;
                        }
                        neighbors[2] = biomeGrid[size - 1, scol];
                    }
                    else
                    {
                        neighbors[2] = biomeGrid[row + 1, col];
                    }

                    // find west neighbor coords
                    if (col == 0)
                    {
                        neighbors[3] = biomeGrid[row, size - 1];
                    }
                    else
                    {
                        neighbors[3] = biomeGrid[row, col - 1];
                    }

                    //find zone of current biome
                    int zone;
                    int distanceFromEquator = row - midPoint;
                    if (distanceFromEquator < 0) distanceFromEquator *= -1;

                    zone = distanceFromEquator / (midPoint / 3);
                    if (zone == 3) zone = 2;

                    biomeGrid[row, col] = DetermineBiome((TemperatureZone)zone, neighbors);
                }

                // count++;
                // if (count % 10 == 0) {
                //     Console.WriteLine(BiomeGridToString());
                // }
            }
        }

        private Biome DetermineBiome(TemperatureZone zone, Biome[] surroundingBiomes)
        {
            // Ice, Tundra, and Taiga should be more near the vertical extremes of our square world (the poles) zone 2

            // Temparate Forest and Grassland should be inbetween the top and middle (between poles and equator) zone 1

            // Desert, Tropical Rainforest, and Savanna should be near the middle (equator) zone 0

            // Oceans should appear in large amounts surrounding landmasses in any zone
            // Freshwater should be in smaller amounts but can appear anywhere in the form of a lake or river
            // rivers should flow from lakes to oceans
            float[] weights;

            switch (zone)
            {
                case TemperatureZone.Cold:
                    weights = ColdWeights;
                    break;
                case TemperatureZone.Temparate:
                    weights = TemparateWeights;
                    break;
                case TemperatureZone.Warm:
                    weights = WarmWeights;
                    break;
                default:
                    throw new FormatException("Zone out of range!");
            }


            for (int i = 0; i < 4; i++)
            {
                Biome b = surroundingBiomes[i];
                if (b > 0)
                { // we don't want undetermined zones being populated
                    if (weights[(int)b] == 0.0f) weights[(int)b] = 10f; // adds chance for biomes to cross temperature zones if there are neighboring zones

                    if (b == Biome.Marine)
                    {
                        weights[(int)b] *= stickiness * 2; // for each surrounding neighbor increase weight
                    }
                    else
                    {
                        weights[(int)b] *= stickiness;
                        weights[(int)Biome.Marine] /= 100f;
                    }
                }
            }

            // Get Total Weight
            float totalWeight = 0.0f;
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            // Generate a random number based on the totalWeight
            float randomNumber = (float)random.NextDouble() * totalWeight;

            // using the random number and weights determine which biome to return
            Biome selectedBiome = Biome.Undetermined;
            for (int i = 0; i < weights.Length; i++)
            {
                float weight = weights[i];
                if (randomNumber < weight)
                {
                    selectedBiome = (Biome)i;
                    break;
                }

                randomNumber -= weight;
            }

            return selectedBiome;
        }


        public string BiomeGridToString() {
            string bgstring = "";
            for (int x = 0; x < size; x++) {
                string row = "";
                for (int y = 0; y < size; y++) {
                    Biome biome = BiomeGrid[x, y];
                    switch(biome) {
                        case Biome.Ice:
                            row += "Ic ";
                            break;
                        case Biome.Taiga:
                            row += "Tg ";
                            break;
                        case Biome.Desert:
                            row += "De ";
                            break;
                        case Biome.Grassland:
                            row += "Gs ";
                            break;
                        case Biome.Marine:
                            row += "~~ ";
                            break;
                        case Biome.Mountain:
                            row += "^^ ";
                            break;
                        case Biome.Savanna:
                            row += "Sv ";
                            break;
                        case Biome.TemperateForest:
                            row += "Tf ";
                            break;
                        case Biome.TropicalRainforest:
                            row += "Rf ";
                            break;
                        case Biome.Tundra:
                            row += "Td ";
                            break;
                        default:
                            row += "Err";
                            break;
                    }
                }
                bgstring += $"{ row }\n";
            }

            return bgstring;
        }
    }
}