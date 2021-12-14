using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Net;
using System.Timers;
using System.Diagnostics;
namespace Final_3
{
    /* Class: Question
     * Author: Jonathan Karcher
     * Purpose: Store a list of questions from the web
     */
    class Question
    {
        public int responceCode;
        public List<QuestionResults> results;
    }
    /* Class: QuestionResults
     * Author: Jonathan Karcher
     * Purpose: Store the Json data of a Question
     */
    class QuestionResults
    {
        public string category;
        public string type;
        public string dificulty;
        public string question;
        public string correct_answer;
        public List<string> incorrect_answers;
    }
    /* Enum: States
     * Author:Jonathan Karcher
     * Purpose: This is states of matter used in the game by both the player and the room
     */
    public enum State : byte
    {
        Ice,
        Liquid_Ice,
        Liquid_Gas,
        Gas
    }
    /* Enum: Actions
     * Author:Jonathan Karcher
     * Purpose: This is the main list of actions that the player can do
     */
    public enum Actions : byte
    {
        Move,
        Gamble,
        ChangeState
    }
    /* Enum: Room
     * Author:Jonathan Karcher
     * Purpose: This is used as an easy readable index of rooms
     * Note: calling the room in wrightline will return the string value not the int index
     */
    public enum Room : byte
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H
    }
    /* Class: S
    *  Author: Jonathan Karcher
    *  Purpose: Used for readability assistance and localize all custom static variables
    */
    public static class S
    {
        public static Random random = new Random();
        // game over trigger
        public static bool gameOver = false;
        // state change trigger
        public static bool changeStateTrigger = false;
        // trivia timer trigger
        public static bool triviaTrigger = false;
        // the room that the player will start at
        public static Room start = Room.A;
        // the room that is the win condition
        public static Room goal = Room.H;
        // the timer used in trivia
        public static Timer triviaTimer = new Timer();
        // the timer to change the state of the room
        public static Timer stateChangeTimer = new Timer();
        // the assembled game map
        public static LinkedList<Node> map = new LinkedList<Node>();
        // the number of rooms moved between
        public static int moveCount = 0;
        // the lock object
        public static Object lockObject = new object();
    }
    /* Class: Node
     * Author: Jonathan Karcher
     * Puropse: This is the node connecting all of one room data
     */
    public class Node
    {
        // the name of the node
        public string name;
        // the state of matter of the room
        public State state;
        public State oldState;
        // list of all edges connected to the room
        //(destination, weight)
        public List<(Room, int)> edge = new List<(Room, int)>();
        /* Constructor: Node
         * Purpose: create a primary node based on data from a adjacency matrix
         * Restrictions: None
         */
        public Node(string name, State state)
        {
            this.name = name;
            this.state = state;
            if (state == State.Gas)
            {
                this.oldState = state;
            }
        }
        /* Method: ChangeState
         * Purpose: Interchange the states of matter
         * Restrictions: None
         */
        public void ChangeState()
        {
            // reset the state timer trigger
            S.changeStateTrigger = false;
            // sequence ice->liquid->gas->liquid
            if(state == State.Ice)
            {
                state = State.Liquid_Gas;
            }
            else if (state == State.Liquid_Gas)
            {
                state = State.Gas;
            }
            else if (state == State.Gas)
            {
                state = State.Liquid_Ice;
            }
            else if(state == State.Liquid_Ice)
            {
                state = State.Ice;
            }
        }
    }
    /* Class: Story
    *  Author: Jonathan Karcher
    *  Purpose: Seperate text display elements from the functional logic
    */
    public class Story
    {
        // player changeing states
        public string playerToIce = "You are now Ice.";
        public string playerToLiquid = "You are now a Liquid.";
        public string playerToGas = "You are now a Gas.";
        public string playerStateChangePrompt = "What state do you want to change to. \n(1) Ice\n(2) Gas)";
        public string playerCantGoBecState = "You are not in the right state to proceed in that direction.";
        public string playerCantGoBecHealth = "You dont have enough health to go that way.";
        // room Descriptions
        public string[,] rooms =
        {
            // room a
            {
                "A large placard sliding around the room says Room A the air is still and hard as if it were frozen.",
                "A large placard floating around the room says Room A and the room ebbs and flows as if made of water.",
                "A large placard floating around the room says Room A and the room dense and formless as if made of gas."
            },
            // room b
            {
                "A large placard sliding around the room says Room B and the walls are slick and smooth as if covered in ice.",
                "A large placard floating around the room says Room B and the room is dark and thick as if made of a viscus fluid.",
                "A large placard floating around the room says Room B and the room is filled with a noxious green hue as if filled with a gas."
            },
            // room c
            {
                "A large placard frozen in the room says Room C and the air is solid like ice.",
                "A large placard floating around the room says Room C and the room is filled with a bubbling liquid.",
                "A large placard floating around the room says Room C and the room seems to be made of steam."
            },
            // room d
            {
                "A large placard sliding around the room says Room D and the air is still and hard as if it were frozen.",
                "A large placard floating around the room says Room D and the room ebbs and flows as if made of water.",
                "A large placard floating around the room says Room D and the room dense and formless as if made of gas."
            },
            // room e
            {
                "A large placard sliding around the room says Room E and the walls are slick and smooth as if covered in ice.",
                "A large placard floating around the room says Room E and the room is dark and thick as if made of a viscus fluid.",
                "A large placard floating around the room says Room E and the room is filled with a noxious  green hue as if filled with a gas."
            },
            // room f
            {
                "A large placard sliding around the room says Room F and the air is solid like ice.",
                "A large placard floating around the room says Room F and the room is filled with a bubbling liquid.",
                "A large placard floating around the room says Room F and the room seems to be made of steam."
            },
            // room g
            {
                "A large placard sliding around the room says Room G and the air is still and hard as if it were frozen.",
                "A large placard floating around the room says Room G and the room ebbs and flows as if made of water.",
                "A large placard floating around the room says Room G and the room dense and formless as if made of gas."
            },
            // room h
            {
                "A large placard sliding around the room says Room H and the walls are slick and smooth as if covered in ice.",
                "A large placard floating around the room says Room H and the room is dark and thick as if made of a viscus fluid.",
                "A large placard floating around the room says Room H and the room is filled with a noxious  green hue as if filled with a gas."
            }
        };
        // player Actions
        public string[] options = new string[3];
        public Story((State, Room, int, string)[,] AdjacencyMatrix)
        {
            options[(int)Actions.Move] = "Move";
            options[(int)Actions.Gamble] = "Gamble";
            options[(int)Actions.ChangeState] = "Change State";
            // build the list of rooms
            for (int i = 0; i < AdjacencyMatrix.GetLength(1); i++)
            {
                Node room = new Node(AdjacencyMatrix[i, 0].Item4, AdjacencyMatrix[i, 0].Item1);
                for (int j = 0; j < AdjacencyMatrix.GetLength(1); j++)
                {
                    if (AdjacencyMatrix[i, j].Item3 != -1)
                    {
                        room.edge.Add((AdjacencyMatrix[i, j].Item2, AdjacencyMatrix[i, j].Item3));
                    }
                }
                S.map.AddLast(room);
            }
        }
        public void Introduction()
        {
            // instructions on how to play
            Console.WriteLine("Welcome to the labyrinth.\n" +
                              "Ahead of you lies 8 rooms.  The rooms can be found in 3 different states Ice, Liquid, and Gas.\n" +
                              "Every 1 second the rooms shift state.  If the room is of the state ice it will transition to liquid.  If the room is of\n" +
                              "the state liquid it will transition to gas.  If the room is of the state gas it will transition to ice.  In order to\n" +
                              "move from one room to another you will need enough health to travel the path and be in the state of the room you are\n" +
                              "trying to go to, the states of the rooms are");
            for (int i = 0; i < S.map.Count(); i++)
            {
                // format the display to just say liquid
                if (S.map.ElementAt<Node>(i).state == State.Liquid_Gas || S.map.ElementAt<Node>(i).state == State.Liquid_Ice)
                {
                    Console.WriteLine("Room " + S.map.ElementAt<Node>(i).name + " is in the state " + "Liquid");
                }
                else
                {
                    Console.WriteLine("Room " + S.map.ElementAt<Node>(i).name + " is in the state " + S.map.ElementAt<Node>(i).state);
                }
            }
            Console.WriteLine("In order to proceed in the game enter the value found in the() for any menu of options.\n" +
                             "Remember there might be rooms that you dont see yet just because you don’t have enough \n" +
                             "health yet to have the option to go down the path.\n" +
                             "In order to get more health you must gamble with what health you have.\n" +
                             "If chose to gamble then you will be presented with a question about video games.\n" +
                             "If you answer correctly then you gain the health that you need to change states and to move around the map.\n" +
                             "But be warned if you answer wrong it could mean your life.\n");
            Console.WriteLine("(Enter) Press enter when you are ready to begin.\n");
            // wait untill the player is ready to start all of the timers
            Console.ReadLine();
        }
    }
    /* Class: Player
     * Author: Jonathan Karcher
     * Purpose: Contain and manage the player data
     */
    public class Player
    {
        // the player state of matter
        State state;
        // the room that the player is in
        public Room position;
        // the players health
        public int playerHP;
        // the standard cost in health of changing a state of matter
        int costOfStateChange = 1;
        /* Constructor: Player
         * Purpose: Initiaize the player to a new game state
         * Restrictions: None
         */
        public Player(Story story)
        {
            state = State.Ice;
            position = S.start;
            playerHP = 5;
        }
        /* Method: ChangeState
         * Purpose: change the player states between ice, liquid, and gas.  If the player is ice or gas then they can only become liquid and if the player is liquid then they can chose either ice or gas
         * Restrictions: None
         */
        public void ChangeState(Story story)
        {
            string liquidInput;
            //if ice
            if (state == State.Ice)
            {
                Console.WriteLine(story.playerToLiquid);
                playerHP -= costOfStateChange;
                state = State.Liquid_Ice;
            }
            //if liquid
            else if (state == State.Liquid_Ice || state == State.Liquid_Gas)
            {
                Console.WriteLine(story.playerStateChangePrompt);
                liquidInput = Console.ReadLine().ToLower();
                if (liquidInput[0] == '1')
                {
                    Console.WriteLine(story.playerToIce);
                    playerHP -= costOfStateChange;
                    state = State.Ice;
                }
                else if (liquidInput[0] == '2')
                {
                    Console.WriteLine(story.playerToGas);
                    playerHP -= costOfStateChange;
                    state = State.Gas;
                }
            }
            // if gas
            else if (state == State.Gas)
            {
                Console.WriteLine(story.playerToLiquid);
                playerHP -= costOfStateChange;
                state = State.Liquid_Ice;
            }
        }
        /* Method: ListOfOptions
         * Purpose: Build the main ingame option list
         * Restrictions: None
         */
        public void ListOfOptions(Story story)
        {

            int optionCounter = 1;
            string input = "";
            int playerChoice = 0;
            List<Actions> options = new List<Actions>();
            // see if the player has the health to move to any adjacent room
            options.Add(Actions.Move);
            // the player always has the option to gamble
            options.Add(Actions.Gamble);
            // if the player has enough health to change state
            if (playerHP > costOfStateChange)
            {
                options.Add(Actions.ChangeState);
            }
            foreach (Actions a in options)
            {
                Console.WriteLine(optionCounter + ") " + story.options[(int)a]);
                optionCounter++;
            }
            // input loop 
            do
            {
                input = Console.ReadLine();
                if (!int.TryParse(input, out playerChoice) || playerChoice < 0 || playerChoice > story.options.Length)
                {
                    Console.WriteLine("Please enter the number next to your choice.");
                }
            } while (!int.TryParse(input, out playerChoice) || playerChoice < 0 || playerChoice > story.options.Length);
            // at this point we can confirm that an acceptable answer has been entered
            int.TryParse(input, out playerChoice);
            playerChoice--;
            switch (playerChoice)
            {
                case (int)Actions.Move:
                    {
                        Move(story);
                        break;
                    }
                case (int)Actions.Gamble:
                    {
                        Gamble(story);
                        break;
                    }
                case (int)Actions.ChangeState:
                    {
                        ChangeState(story);
                        break;
                    }
            }
        }
        /* Method: Gamble
         * Purpose: Manage the players choice to gamble their health
         * Restrictions: None
         */
        public void Gamble(Story story)
        {
            // question reference
            Question question; 
            // a list of answers
            LinkedList<string> answers = new LinkedList<string>();
            // a queue for the mixed up list of answers
            Queue<string> answersMixed = new Queue<string>();
            // a string representing the value stored in an answers node
            string node;
            // an int for the number of elements stored in the answers list
            int answerCout;
            // a reference for a random index to mix up the answers
            int randomIndex;
            // a reference to display the input options for answering the questions
            int index;
            // a reference used to find the right answer
            int indexToCheck;
            // url where questions originate
            string url = "https://opentdb.com/api.php?amount=1&category=15&type=multiple";
            // the player input
            string playerAnswer = "";
            // the amput of health to wager initialized to zero
            int amountToWager = 0;
            do
            {
                Console.WriteLine("How much life would you like to wager?");
                playerAnswer = Console.ReadLine();
                Console.WriteLine();
                // if they did enter a number but dont have that much life
                if (int.TryParse(playerAnswer, out amountToWager))
                {
                    // if they try to barter health that they dont have
                    if (amountToWager > playerHP)
                    {
                        Console.WriteLine("You dont have that much life to wager.\n");
                    }
                    // if they try to cheat the system by entering a negative number and intentionally getting questions wrong
                    if (amountToWager < 0)
                    {
                        // I will still alow a value of 0 so the player can try a few questions before risking death
                        Console.WriteLine("HA, nice try but no, entering a negative value wont get you out of this one.\n");
                    }
                }
                // if they didnt enter a number
                else
                {
                    Console.WriteLine("Thats not an integer.\n");
                }
            } while (!int.TryParse(playerAnswer, out amountToWager) || amountToWager > playerHP || amountToWager < 0);
            // streams to manage getting the question and answers from the web
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            // read the question and answers from the web
            string s = reader.ReadToEnd();
            reader.Close();
            // make the string human readable
            question = JsonConvert.DeserializeObject<Question>(s);
            question.results[0].question = HttpUtility.HtmlDecode(question.results[0].question);
            question.results[0].correct_answer = HttpUtility.HtmlDecode(question.results[0].correct_answer);
            for (int i = 0; i < question.results[0].incorrect_answers.Count; i++)
            {
                question.results[0].incorrect_answers[i] = HttpUtility.HtmlDecode(question.results[0].incorrect_answers[i]);
            }
            // add the correct answer to the list of answers
            answers.AddFirst(question.results[0].correct_answer);
            // add the incorrect answers to the list of answers
            for (int i = 0; i < question.results[0].incorrect_answers.Count; i++)
            {
                answers.AddLast(question.results[0].incorrect_answers[i]);
            }
            // store the number of elements stored in the answers list
            answerCout = answers.Count;
            // mix up the elements stored in the answers list and add them to the answersMixed queue
            for (int i = 0; i < answerCout; i++)
            {
                // get a random int based on the total number of elements in the answers list
                randomIndex = S.random.Next(answers.Count);
                // get the value of the node at the random int
                node = answers.ElementAt<string>(randomIndex);
                // remove the node that contains the string
                answers.Remove(node);
                // add the string to the queue
                answersMixed.Enqueue(node);
            }
            // reset the time up timer
            S.triviaTrigger = false;
            Program.ResetTimer();
            // output the question
            Console.WriteLine(question.results[0].question + "\n");
            // output all of the mixed answers
            index = 1;
            foreach (string answer in answersMixed)
            {
                Console.WriteLine("(" + index + ") " + answer);
                index++;
            }
            // because im bad at trivia
            //Console.WriteLine("(Correct) " + question.results[0].correct_answer);
            // get the answer from the player
            playerAnswer = Console.ReadLine();
            if (!playerAnswer.Equals("1") && !playerAnswer.Equals("2") && !playerAnswer.Equals("3") && !playerAnswer.Equals("4"))
            {
                if (!S.triviaTrigger)
                {
                    Console.WriteLine("Its your life your playing with.\n");
                }
            }
            //figure out which index is the right answer
            indexToCheck = 0;
            foreach (string answer in answersMixed)
            {
                indexToCheck++;
                if (answer.Contains(question.results[0].correct_answer))
                {
                    break;
                }
            }
            // if the player has runout of time then force their answer to be wrong
            if (S.triviaTrigger)
            {
                playerAnswer = question.results[0].incorrect_answers[0];
            }
            // if the player enters a correct value
            if (playerAnswer.Equals("" + indexToCheck))
            {
                Console.WriteLine();
                playerHP += amountToWager;
                // output a story element
                Console.WriteLine("Congratulations your Right! ");
            }
            // if the player enters an incorrect value
            else
            {
                Console.WriteLine();
                playerHP -= amountToWager;
                if (!S.triviaTrigger)
                {
                    // output a story element
                    Console.WriteLine("Wrong, now you must pay!");
                }
            }
            S.triviaTimer.Stop();
        }
        /* Method: Move
         * Purpose: Manage the players choice to move form one room to another
         * Restrictions: None
         */
        public void Move(Story story)
        {
            // player input
            string input;
            // player input after formating
            int numberInput = 0;
            // the room we are in
            Node room = S.map.ElementAt<Node>((int)position);
            // a local list of all the available rooms
            List<(Room, int)> avaliableRooms = new List<(Room, int)>();
            Console.WriteLine("The rooms available are to you are ");
            // go through all of the avalable edges
            for(int i = 0; i < room.edge.Count(); i++)
            {
                // if the player has enough health to travel
                if (playerHP - 1 > room.edge.ElementAt<(Room, int)>(i).Item2)
                {
                    // add the edge to the list of available options
                    avaliableRooms.Add(room.edge.ElementAt<(Room, int)>(i));
                }
            }
            int counter = 0;
            // output all of their options
            foreach( (Room, int) e in avaliableRooms)
            {
                Console.Write("(" + (counter + 1) + ") " + e.Item1 + " ");
                counter++;
            }
            // input loop
            do
            {
                input = Console.ReadLine();
                if (!int.TryParse(input, out numberInput) || numberInput < 0 || numberInput > story.options.Length)
                {
                    Console.WriteLine("Please enter the number next to your choice.");
                }
            } while (!int.TryParse(input, out numberInput) || numberInput < 0 || numberInput > story.options.Length);
            // reformat the input to be usable
            numberInput -= 1;

            // if the player has entered a value wait check uf they are the state that they entered at the time they entered it
            lock (S.lockObject)
            {
                //S.map.ElementAt<Node>((int)room.edge.ElementAt<(Room, int)>(numberInput).Item1) <-- node in the map
                //(int)room.edge.ElementAt<(Room, int)>(numberInput).Item2 <-- weight of the edge between this position and the node at the end of the edge
                //room.edge.ElementAt<(Room, int)>(numberInput).Item1 <-- the room at the end of the edge

                // if we are in the right state
                if (state == S.map.ElementAt<Node>((int)room.edge.ElementAt<(Room, int)>(numberInput).Item1).state)
                {
                    // if the player can go down the path without dieing
                    if ((playerHP - 1) > room.edge.ElementAt<(Room, int)>(numberInput).Item2)
                    {
                        // reduce the amout of health found in the edge
                        playerHP -= room.edge.ElementAt<(Room, int)>(numberInput).Item2;
                        // set the position of the player to the position chosen
                        position = room.edge.ElementAt<(Room, int)>(numberInput).Item1;
                        // wright the description of the room
                        Console.WriteLine(story.rooms[(int)room.edge.ElementAt<(Room, int)>(numberInput).Item1, (int)S.map.ElementAt<Node>((int)room.edge.ElementAt<(Room, int)>(numberInput).Item1).state]);
                        // increase the move couter
                        S.moveCount++;
                    }
                }
                // inform the player that they are not in the right state to proceed
                else
                {
                    Console.WriteLine(story.playerCantGoBecState);
                }
            }
        }
        /* Method: DeadCheck
         * Purpose: if the player HP is at 0 or less then the player is dead and the game is over
         * Restrictions: None
         */
        public void DeadCheck()
        {
            if(playerHP <= 0)
            {
                S.gameOver = true;
            }
        }
    }
    /* Class: Program
     * Author: Jonathan Karcher
     * Purpose: Main entery class for the program
     */
    class Program
    {
        /* Method: Main
         * Purpose: Main entery point for the program
         * Restrictions: None
         */
        static void Main(string[] args)
        {
            // Initialize the game
            //                          [destination]
            // [origion] (state of the room, index of the room, weigth of this edge, name)
            // Note: the name of the room is only relavent in the first line and that is only used in the intorductions generation of the rooms themselves. 
            (State, Room, int, string)[,] AdjacencyMatrix =
            {
               //          A                                    B                                   C                                   D                                   E                                   F                                   G                                   H
               /* 0 A*/ { (State.Ice, Room.A, -1, "A"),        (State.Ice, Room.B,  1, ""),        (State.Ice, Room.C,  5, ""),        (State.Ice, Room.D, -1, ""),        (State.Ice, Room.E, -1, ""),        (State.Ice, Room.F, -1, ""),        (State.Ice, Room.G, -1, ""),        (State.Ice, Room.H, -1, "") },
               /* 1 B*/ { (State.Liquid_Ice, Room.A, -1, "B"), (State.Liquid_Ice, Room.B, -1, ""), (State.Liquid_Ice, Room.C, -1, ""), (State.Liquid_Ice, Room.D,  1, ""), (State.Liquid_Ice, Room.E, -1, ""), (State.Liquid_Ice, Room.F,  7, ""), (State.Liquid_Ice, Room.G, -1, ""), (State.Liquid_Ice, Room.H, -1, "") },
               /* 2 C*/ { (State.Gas, Room.A, -1, "C"),        (State.Gas, Room.B, -1, ""),        (State.Gas, Room.C, -1, ""),        (State.Gas, Room.D,  0, ""),        (State.Gas, Room.E,  2, ""),        (State.Gas, Room.F, -1, ""),        (State.Gas, Room.G, -1, ""),        (State.Gas, Room.H, -1, "") },
               /* 3 D*/ { (State.Ice, Room.A, -1, "D"),        (State.Ice, Room.B,  1, ""),        (State.Ice, Room.C,  0, ""),        (State.Ice, Room.D, -1, ""),        (State.Ice, Room.E, -1, ""),        (State.Ice, Room.F, -1, ""),        (State.Ice, Room.G, -1, ""),        (State.Ice, Room.H, -1, "") },
               /* 4 E*/ { (State.Liquid_Ice, Room.A, -1, "E"), (State.Liquid_Ice, Room.B, -1, ""), (State.Liquid_Ice, Room.C,  2, ""), (State.Liquid_Ice, Room.D, -1, ""), (State.Liquid_Ice, Room.E, -1, ""), (State.Liquid_Ice, Room.F, -1, ""), (State.Liquid_Ice, Room.G,  2, ""), (State.Liquid_Ice, Room.H, -1, "") },
               /* 5 F*/ { (State.Gas, Room.A, -1, "F"),        (State.Gas, Room.B, -1, ""),        (State.Gas, Room.C, -1, ""),        (State.Gas, Room.D, -1, ""),        (State.Gas, Room.E, -1, ""),        (State.Gas, Room.F, -1, ""),        (State.Gas, Room.G, -1, ""),        (State.Gas, Room.H,  4, "") },
               /* 6 G*/ { (State.Ice, Room.A, -1, "G"),        (State.Ice, Room.B, -1, ""),        (State.Ice, Room.C, -1, ""),        (State.Ice, Room.D, -1, ""),        (State.Ice, Room.E,  2, ""),        (State.Ice, Room.F,  1, ""),        (State.Ice, Room.G, -1, ""),        (State.Ice, Room.H, -1, "") },
               /* 7 H*/ { (State.Liquid_Ice, Room.A, -1, "H"), (State.Liquid_Ice, Room.B, -1, ""), (State.Liquid_Ice, Room.C, -1, ""), (State.Liquid_Ice, Room.D, -1, ""), (State.Liquid_Ice, Room.E, -1, ""), (State.Liquid_Ice, Room.F, -1, ""), (State.Liquid_Ice, Room.G, -1, ""), (State.Liquid_Ice, Room.H, -1, "") }
            };
            // create a story object
            Story story = new Story(AdjacencyMatrix);
            // create a player
            Player player = new Player(story);
            // display the introduction once
            // note the game requiers input here in order to proceed
            story.Introduction();
            // starts the initial timer
            Stopwatch gameTime = new Stopwatch();
            // prime the state change timer
            ResetStateChangeTimer();
            gameTime.Start();
            while (!S.gameOver)
            {
                Console.WriteLine("Your Health: " + player.playerHP);
                player.ListOfOptions(story);
                // change the state of the rooms
                if(S.changeStateTrigger)
                {
                    foreach (Node r in S.map)
                    {
                        //r.ChangeState();
                    }
                    ResetStateChangeTimer();
                }
                // did we win
                if(player.position == S.goal)
                {
                    S.gameOver = true;
                    gameTime.Stop();
                    TimeSpan gameTimePassed = gameTime.Elapsed;
                    Console.WriteLine(Math.Round(gameTimePassed.TotalSeconds,0)+"\n"+S.moveCount);
                }
                // check if the player is dead at the end of evey input cycle
                player.DeadCheck();
            }
        }
        /* Method: ResetStateChangeTimer
         * Purpose: Reset the game timer to 1 second
         * Restrictions: None
         */
        public static void ResetStateChangeTimer()
        {
            // set the timer to 1 second
            S.stateChangeTimer = new Timer(1000);
            // create an event handeler
            ElapsedEventHandler elapsedEventHandler = new ElapsedEventHandler(ChangeRoomStates);
            // add the event handeler to the elapsed listener
            S.stateChangeTimer.Elapsed += elapsedEventHandler;
            // start the timer
            S.stateChangeTimer.Start();
        }
        /* Method: ChangeRoomStates
         * Purpose: trigger the nodes to switch their state
         * Restrictions: None
         */
        static void ChangeRoomStates(Object source, ElapsedEventArgs e)
        {
            // set the room state trigger to switch state
            S.changeStateTrigger = true;
            // stop the timer 
            S.stateChangeTimer.Stop();
        }
        /* Method: ResetTimer
         * Purpose: Reset the trivia timer to 15 seconds
         * Restrictions: None
         */
        public static void ResetTimer()
        {
            // set the timer to 15 seconds
            S.triviaTimer = new Timer(15000);
            // create an event handeler
            ElapsedEventHandler elapsedEventHandler = new ElapsedEventHandler(TimeIsUp);
            // add the event handeler to the elapsed listener
            S.triviaTimer.Elapsed += elapsedEventHandler;
            // start the timer
            S.triviaTimer.Start();
        }
        /* Method: TimeIsUp
         * Purpose: Inform the player that they have run out of time, tell the player how to proceed
         * Restrictions: None
         */
        static void TimeIsUp(Object source, ElapsedEventArgs e)
        {
            // tell the user the time is up
            Console.WriteLine("Too late now pay up.");
            // tell the user how to proceed
            Console.WriteLine("(Enter)");
            // set the timer trigger to be over
            S.triviaTrigger = true;
            // stop the timer 
            S.triviaTimer.Stop();
        }
    }
}
