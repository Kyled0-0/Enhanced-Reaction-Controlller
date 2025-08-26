using System;

namespace EnhancedReactionMachine
{
    class Tester
    {
        private static IController controller;
        private static IGui gui;
        private static string displayText;
        private static int randomNumber;
        private static int passed = 0;

        static void Main(string[] args)
        {
            // run simple test
            SimpleTest();
            Console.WriteLine("\n=====================================\nSummary: {0} tests passed out of 38", passed);
            Console.ReadKey();
        }

        private static void SimpleTest()
        {
            //Construct a ReactionController
            controller = new EnhancedReactionController();
            gui = new DummyGui();

            //Connect them to each other
            gui.Connect(controller);
            controller.Connect(gui, new RndGenerator());

            //Reset the components()
            gui.Init();

            //Test the Enhanced EnhancedReactionController
            //Test cheat case and basic resposes
            //IDLE
            DoReset('A', controller, "Insert coin");
            DoGoStop('B', controller, "Insert coin");
            DoTicks('C', controller, 1, "Insert coin");

            //CoinInserted
            DoInsertCoin('D',controller, "Press GO!");

            //READY
            DoTicks('E', controller, 1, "Press GO!");
            DoInsertCoin('F', controller, "Press GO!");

            //READY, wait too long >10s
            DoTicks('G',controller,1000,"Insert coin");

            //WAIT
            DoInsertCoin('H',controller, "Press GO!");
            randomNumber = 117;
            DoGoStop('I',controller,"Wait...");

            //WAIT - cheated
            DoGoStop('J',controller,"Insert coin");



            //Game1 (perfect game loop and no skipping wait time)
            //IDLE -> READY -> WAIT -> RUN -> STOP
            DoReset('K', controller, "Insert coin");
            DoInsertCoin('L',controller, "Press GO!");
            randomNumber = 160;
            DoGoStop('M',controller,"Wait...");
            //wait ticks
            DoTicks('N', controller, randomNumber - 1, "Wait...");
            //run ticks
            DoTicks('O',controller,60,"Time #1: 0.59");
            //goStop
            DoGoStop('P',controller,"Time #1: 0.59");
            //check display duration (3s)
            DoTicks('Q',controller,300,"Press GO!");


            //Game2 (perfect game loop and no skipping wait time)
            randomNumber = 120;
            DoTicks('R',controller,300,"Wait...");
            //waitTicks
            DoTicks('S', controller, randomNumber-1, "Wait...");
            //run ticks
            DoTicks('T', controller, 111, "Time #2: 1.10");
            //goStop
            DoGoStop('U',controller,"Time #2: 1.10");     
            //check display duration (3s)
            DoTicks('V',controller,300,"Press GO!");


            //Game3 (perfect game loop and no skipping wait time)
            randomNumber = 123;
            DoTicks('W',controller,300,"Wait...");
            DoTicks('X', controller, randomNumber + 200, "Time #3: 2.00");
            DoTicks('Y', controller, 1, "Time #3: 2.00");
            DoTicks('Z',controller,300,"Average Time: 1.23");


            //Game loop that skip all wait time (test skip wait time)
            gui.Init();
            DoReset('a',controller,"Insert coin");
            DoInsertCoin('b',controller,"Press GO!");
            randomNumber = 160;
            DoGoStop('c',controller,"Wait...");
            DoTicks('d', controller, randomNumber + 156, "Time #1: 1.56");
            //goStop - displaying game#1 time
            DoGoStop('e',controller,"Time #1: 1.56");


            //skip wait to GAME#2 time
            DoGoStop('f',controller,"Time #2: 0.00");
            DoTicks('g', controller,60, "Time #2: 0.60");
            //goStop - displaying game#2 time
            DoGoStop('h',controller,"Time #2: 0.60");


            //skip wait to GAME#3 time
            DoGoStop('i',controller,"Time #3: 0.00");
            DoTicks('j', controller,190, "Time #3: 1.90");
            //goStop - displaying game#2 time
            DoGoStop('k',controller,"Time #3: 1.90");

            //skip wait to game#3 time
            DoGoStop('l',controller,"Average Time: 1.35");

            //reset game immediately
            DoGoStop('m',controller,"Insert coin");


            //testing cheat detect for other wait time other than game#1
            gui.Init();
            DoReset('n',controller,"Insert coin");
            DoInsertCoin('o',controller,"Press GO!");
            randomNumber = 160;
            DoGoStop('p',controller,"Wait...");
            DoTicks('q', controller, randomNumber + 200, "Time #1: 2.00");
            //goStop - displaying game#1 time
            DoTicks('r',controller,1,"Time #1: 2.00");


            //skip wait to GAME#2 time
            DoGoStop('s',controller,"Time #2: 0.00");
            DoTicks('t', controller,160, "Time #2: 1.60");
            //goStop - displaying game#2 time
            DoGoStop('u',controller,"Time #2: 1.60");
            DoTicks('v',controller,300,"Press GO!");
            DoTicks('w',controller,300,"Wait...");
            //cheated
            DoGoStop('x',controller,"Insert coin");

            gui.Init();



            
            




            

        }

        private static void DoReset(char ch, IController controller, string msg)
        {
            try
            {
                controller.Init();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoGoStop(char ch, IController controller, string msg)
        {
            try
            {
                controller.GoStopPressed();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoInsertCoin(char ch, IController controller, string msg)
        {
            try
            {
                controller.CoinInserted();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoTicks(char ch, IController controller, int n, string msg)
        {
            try
            {
                for (int t = 0; t < n; t++) controller.Tick();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void GetMessage(char ch, string msg)
        {
            if (msg.ToLower() == displayText.ToLower())
            {
                Console.WriteLine("test {0}: passed successfully", ch);
                passed++;
            }
            else
                Console.WriteLine("test {0}: failed with message ( expected {1} | received {2})", ch, msg, displayText);
        }

        private class DummyGui : IGui
        {

            private IController controller;

            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                displayText = "?reset?";
            }

            public void SetDisplay(string msg)
            {
                displayText = msg;
            }
        }

        private class RndGenerator : IRandom
        {
            public int GetRandom(int from, int to)
            {
                return randomNumber;
            }
        }

    }

}
