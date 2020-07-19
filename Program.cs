using System;
using System.Globalization;

namespace worldgenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            WorldGen worldGen = GetNewWorldGenerator();
            bool loop = true;

            while (loop) {
                Console.WriteLine("Type Command: (h for help)");
                string command = Console.ReadLine();
                bool fail;

                switch(command.ToUpper()) {
                    case "H":   // print commands
                    case "HELP":
                        Console.WriteLine("(H)elp: you are here");
                        Console.WriteLine("(W)orld: restart the world");
                        Console.WriteLine("Generate(B)iomes: generate Biomes grid");
                        Console.WriteLine("(P)rint: print the world in the console");
                        Console.WriteLine("(SE)ed: change the seed");
                        Console.WriteLine("(S)ize: change the size");
                        Console.WriteLine("(I)nfo: View seed, size, and weights");
                        Console.WriteLine("(ST)ickiness: Change how sticky biomes are (lower numbers will create smaller biomes)");
                        Console.WriteLine("(Q)uit: quit");
                        break;

                    case "W":   // create new world params
                    case "WORLD":
                        worldGen = GetNewWorldGenerator();
                        break;

                    case "B":
                    case "GENERATEBIOMES":
                        worldGen.GenerateBiomes();
                        break;

                    case "P":
                    case "PRINT":
                        Console.WriteLine(worldGen.BiomeGridToString());
                        Console.WriteLine(worldGen.SeedInfo);
                        break;

                    case "S":
                    case "SIZE":
                        fail = true;
                        while (fail)  {
                            Console.WriteLine("Input an int for size");
                            fail = !int.TryParse(Console.ReadLine(), out worldGen.size);
                            if (fail) {
                                Console.WriteLine("Invalid size!");
                            }
                        }
                        break;

                    case "SE":
                    case "SEED":
                        fail = true;
                        while (fail) {
                            Console.WriteLine("Input a valid hex string for new seed");
                            string seed = Console.ReadLine();
                            
                            if (seed.StartsWith("0x")) {
                                seed = seed.Substring(2);
                            }

                            fail = !int.TryParse(seed,
                                                System.Globalization.NumberStyles.HexNumber,
                                                CultureInfo.InvariantCulture,
                                                out worldGen.seed);

                            if (fail) {
                                Console.WriteLine("Invalid Hex String");
                            }
                        }
                        break;

                    case "I":
                    case "Info":
                        Console.WriteLine(worldGen.SeedInfo);
                        break;

                    case "Q":
                    case "QUIT":
                        loop = false;
                        break;

                    case "ST":
                    case "STICKINESS":
                        fail = true;
                        while (fail)  {
                            Console.WriteLine("Input a float for stickiness value");
                            fail = !float.TryParse(Console.ReadLine(), out worldGen.stickiness);
                            if (fail) {
                                Console.WriteLine("Invalid stickiness!");
                            }
                        }
                        break;

                    default:
                        Console.WriteLine("Invalid Command");
                        break;
                }
            }
        }

        private static WorldGen GetNewWorldGenerator() {
            int size;
            string seed;
            WorldGen worldGen;
            Console.WriteLine("Input seed (enter for random seed):");
            seed = Console.ReadLine();
            Console.WriteLine($"Input size (enter for default of { WorldGen.DefaultSize }):");
            bool customSize = int.TryParse(Console.ReadLine(), out size);
            if (String.IsNullOrWhiteSpace(seed)) {
                worldGen = size <= 0 ? new WorldGen() : new WorldGen(size);
            }
            else {
                worldGen = size <= 0 ? new WorldGen(seed, null) : new WorldGen(seed, size);
            }
            
            Console.WriteLine(worldGen.SeedInfo);
            return worldGen;
        }
    }
}
