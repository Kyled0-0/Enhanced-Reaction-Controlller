using System;
using System.Timers;

namespace EnhancedReactionMachine
{
    class EnhancedReactionMachine
    {
        const string TOP_LEFT_JOINT = "┌";
        const string TOP_RIGHT_JOINT = "┐";
        const string BOTTOM_LEFT_JOINT = "└";
        const string BOTTOM_RIGHT_JOINT = "┘";
        const string TOP_JOINT = "┬";
        const string BOTTOM_JOINT = "┴";
        const string LEFT_JOINT = "├";
        const string JOINT = "┼";
        const string RIGHT_JOINT = "┤";
        const char HORIZONTAL_LINE = '─';
        const char PADDING = ' ';
        const string VERTICAL_LINE = "│";

        static private IController controller;
        static private IGui gui;

        static void Main(string[] args)
        {
            // Make a menu
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0}{1}{2}", TOP_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), TOP_RIGHT_JOINT);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", LEFT_JOINT, new string(HORIZONTAL_LINE, 50), RIGHT_JOINT);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", VERTICAL_LINE, new string(' ', 50), VERTICAL_LINE);
            Console.WriteLine("{0}{1}{2}", BOTTOM_LEFT_JOINT, new string(HORIZONTAL_LINE, 50), BOTTOM_RIGHT_JOINT);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.SetCursorPosition(5, 6);
            Console.Write("{0,-20}", "- For Insert Coin press SPACE");
            Console.SetCursorPosition(5, 7);
            Console.Write("{0,-20}", "- For Go/Stop action press ENTER");
            Console.SetCursorPosition(5, 8);
            Console.Write("{0,-20}", "- For Exit press ESC");

            // Create a time for Tick event
            System.Timers.Timer timer = new System.Timers.Timer(10);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            // Connect GUI with the Controller and vice versa
            controller = new EnhancedReactionController();
            gui = new Gui();
            gui.Connect(controller);
            controller.Connect(gui, new RandomGenerator());

            //Reset the GUI
            gui.Init();
            // Start the timer
            timer.Enabled = true;

            // Run the menu
            bool quitePressed = false;
            while (!quitePressed)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        controller.GoStopPressed();
                        break;
                    case ConsoleKey.Spacebar:
                        controller.CoinInserted();
                        break;
                    case ConsoleKey.Escape:
                        quitePressed = true;
                        break;
                }
            }
        }


        // This event occurs every 10 msec
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            controller.Tick();
        }

        // Internal implementation of Random Generator
        private class RandomGenerator : IRandom
        {
            Random rnd = new Random(100);

            public int GetRandom(int from, int to)
            {
                return rnd.Next(to - from) + from;
            }
        }

        // Internal implementation of GUI
        private class Gui : IGui
        {
            private IController controller;
            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                SetDisplay("Start your game!");
            }

            public void SetDisplay(string text)
            {
                PrintUserInterface(text);
            }

            private void PrintUserInterface(string text)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.SetCursorPosition(15, 2);
                Console.Write("{0,-20}", text);
                Console.SetCursorPosition(0, 10);
            }
        }
    }

    internal class EnhancedReactionController : IController
    {
        const int WAITFORGO = 1000;//10s
        const int DISPLAYTIME = 300;//3s

        const int DISPLAYAVGTIME = 500;

        const int GAMEAMOUNT = 3;//3 games
        
        private enum State {IDLE,READY,WAIT,RUN,STOP,AVERAGE};
        private State currentState = State.IDLE;

        private IGui gui;
        private IRandom rnd;

        private int waitTime = 0;
        private int waitGo = 0;
        private int elapsedTime = 0;
        private int displayDuration = 0;

        private int GameCount = 0;

        private double TotalTime = 0;

        public void Connect(IGui gui, IRandom rnd)
        {
            this.gui = gui;
            this.rnd = rnd;
        }

        //react when a coin is inserted
        public void CoinInserted()
        {
            if(currentState == State.IDLE)//new game
            {
                GameCount = 0;
                TotalTime = 0;
                waitGo = 0;
                elapsedTime = 0;
                displayDuration = 0;

                gui.SetDisplay("Press GO!");
                currentState = State.READY;
            }
        }

        public void GoStopPressed()
        {
            switch(currentState)
            {
                case State.READY:
                {
                    if(GameCount > 0)//skip wait
                        currentState = State.RUN;
                    else
                    {
                        waitTime = rnd.GetRandom(100,250);
                        gui.SetDisplay("Wait...");
                        currentState = State.WAIT;
                    }
                    
                    break;
                }

                case State.WAIT:        
                { 
                    //cheated
                    gui.SetDisplay("Insert coin");
                    currentState = State.IDLE;
                    break;
                }

                case State.RUN:
                {
                    //show result
                    currentState = State.STOP;
                    double seconds = elapsedTime / 100.0;
                    gui.SetDisplay($"Time #{GameCount+1}: {seconds:F2}");
                    TotalTime += seconds;//add time when pressed
                    GameCount++;
                    break;
                }

                case State.STOP:
                {
                    if(GameCount == GAMEAMOUNT)//game over
                    {
                        double Average = TotalTime / GameCount;
                        gui.SetDisplay($"Average Time: {Average:F2}");
                        currentState = State.AVERAGE;
                    }
                    else//continue next game
                    {
                        elapsedTime = 0;
                        displayDuration = 0;
                        gui.SetDisplay($"Time #{GameCount+1}: 0.00");
                        currentState = State.RUN;
                    }
                        
                    break;
                }

                case State.AVERAGE:
                {
                    gui.SetDisplay("Insert coin");
                    currentState = State.IDLE;
                    break;
                }
            }
        }

        public void Tick()
        {
            switch(currentState)
            {
                case State.IDLE:
                    gui.SetDisplay("Insert coin");
                    break;

                case State.READY:
                {
                    gui.SetDisplay("Press GO!");
                    waitGo++;
                    if(GameCount == 0)
                    {
                        if(waitGo >= WAITFORGO) //reset if wait too long for pressgo, only apply to first game
                        {
                            waitGo = 0;
                            currentState = State.IDLE;
                        }
                    }
                    else
                    {
                        if(waitGo >= DISPLAYTIME)//3s then switch to wait
                        {
                            waitTime = rnd.GetRandom(100,250);
                            waitGo = 0;
                            gui.SetDisplay("Wait...");
                            currentState = State.WAIT;
                        }
                            
                    }

                    break;
                }
                    
                case State.WAIT:
                {
                    waitTime--;
                    if(waitTime == 0)//start reaction timer
                    {
                        gui.SetDisplay($"Time #{GameCount+1}: 0.00");
                        currentState = State.RUN;
                    }
                    else
                        gui.SetDisplay("Wait...");

                    break;
                }

                case State.RUN:
                {
                    elapsedTime++;
                    double seconds = elapsedTime/100.0;
                    gui.SetDisplay($"Time #{GameCount+1}: {seconds:F2}");
                    if(seconds == 2.0)
                    {
                        currentState = State.STOP;
                        TotalTime += seconds;
                        GameCount++;
                    }
                        
                    break;
                }

                
                case State.STOP:
                {
                    displayDuration++;
                    if(displayDuration >= DISPLAYTIME)
                    {
                        if(GameCount >= GAMEAMOUNT) //display average
                        {
                            currentState = State.AVERAGE;
                            displayDuration = 0;
                        }
                        else//continue next game
                        {
                            elapsedTime = 0;
                            displayDuration = 0;

                            gui.SetDisplay("Press GO!");
                            currentState = State.READY;
                        }
                    }
                    else //display time
                    {
                        double seconds = elapsedTime/100.0;
                        gui.SetDisplay($"Time #{GameCount}: {seconds:F2}");
                    }
                    break;
                }


                case State.AVERAGE:
                {
                    displayDuration++;
                    if(displayDuration >= DISPLAYAVGTIME)
                    {
                        gui.SetDisplay("Insert coin");
                        currentState = State.IDLE;
                    }
                    else
                    {
                        double Average = TotalTime / GameCount;
                        gui.SetDisplay($"Average Time: {Average:F2}");
                    }
                    break;
                }
                
            }
        }
        
        public void Init()
        {
            gui.SetDisplay("Insert coin");
            currentState = State.IDLE;
        }
    }
}
