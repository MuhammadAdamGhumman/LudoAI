using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LGClient
{
    class Program
    {

        static void Main(string[] args)
        {
            AgentManager ag = new AgentManager();
            ag.StartComm();

        }//end of Main
    }//End of Program



    public class AgentManager
    {
        Random rnd = new Random();

        int myid = 1;
        List<AgentState> AgentStates = new List<AgentState>();
        AgentState myState = new AgentState();

        public void StartComm()
        {
            Boolean flag = true;
            do
            {
                var messageFromServer = Console.ReadLine();

                //var messageFromServer = "TRN;1;664;-1 -1 -1 -1:-1 -1 -1 -1:-1 -1 -1 -1:-1 -1 -1 -1";
                //Console.WriteLine("Hello worlds");
                //return;

                var parts = messageFromServer.Split(";".ToCharArray());

                if (parts.Length > 0)
                {
                    var command = parts[0];
                    int id = 0;

                    Int32.TryParse(parts[1], out id);

                    if (command == "INI")
                    {
                        myid = id;
                        SetBoardState(parts[3]);

                        Console.WriteLine("ACK;{0}", myid);
                    }//end of INI
                    else if (command == "END")
                    {
                        if (id == myid)
                        {
                            flag = false;
                            //Console.WriteLine("ACK;{0}", myid);
                        }
                    }//end of QIT
                    else if (command == "TRN")
                    {
                        if (id == myid)
                        {
                            var moves = parts[2].Trim();
                            SetBoardState(parts[3]);
                            string updatedState = myState.GetState();

                            if (moves != "666" && moves != "000")
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    var score = Int16.Parse(moves[i].ToString());
                                    if (score == 6)
                                    {
                                        CheckDeadPiecesAndMove(score);
                                    }
                                    else
                                    {
                                        MovePieceInBoard(score);
                                        break;
                                    }
                                }
                                updatedState = myState.GetState();
                            }

                            Console.WriteLine("RSP;{0};{1}", myid, updatedState);
                        }
                    }//end of TRN
                }//end of parts length
                //Console.WriteLine(myid);

            } while (flag);
        }//end of  StartComm

        void SetBoardState(String stateStr)
        {
            AgentStates = new List<AgentState>();

            var states = stateStr.Split(":".ToCharArray());
            for(int i=0;i<4;i++)
            {
                AgentState stateObj = new AgentState(states[i]);
                stateObj.AgentId = i + 1;
                AgentStates.Add(stateObj);
            }

            myState = AgentStates.Where(p => p.AgentId == myid).FirstOrDefault();
        }

        void CheckDeadPiecesAndMove(Int16 score)
        {
            if (score == 6)
            {
                Boolean deadPieceFound = false;

                for (int i = 0; i < 4; i++)
                {
                    if (myState.Pieces[i] == -1)
                    {
                        myState.Pieces[i] = 0;
                        deadPieceFound = true;
                        break;
                    }
                }//end of for loop

                if (deadPieceFound == false)
                {
                    MovePieceInBoard(score);
                }
            }//end of score == 6
            else
            {
                MovePieceInBoard(score);
            }//end of else
        }// end of CheckDeadPieces


        void MovePieceInBoard(Int16 score)
        {
            //var r = rnd.Next(0, 4); //random piece to move

            for (int p1 = 0; p1 < 4; p1++) // loop for my agent's pieces
            {
                //var index = (p1 + r) % 4;

                var index = p1;

                if (myState.Pieces[index] > -1 && myState.Pieces[index] < 56 && (myState.Pieces[index] + score) <= 56)
                {
                    for (int o = 0; o < 4; o++) // loop for all opponent agents
                    {
                        if (AgentStates[o].AgentId != myid)
                        {
                            var opponent = AgentStates[o];

                            for (int op = 0; op < 4; op++) // loop for an opponent agent's pieces
                            {
                                if (opponent.Pieces[op] >= 0 && opponent.Pieces[op] < 51)
                                {
                                    int piece = (opponent.Pieces[op] - ((myid - AgentStates[o].AgentId)*13)) % 52;
                                    
                                    if (piece > myState.Pieces[index])
                                    {
                                        if (myState.Pieces[index] + score == piece) // check to see if opponent piece is killable
                                        {
                                            myState.Pieces[index] += score;

                                            return;
                                        }
                                    }
                                    else if ((myState.Pieces[index] - piece) <= 6) // checks to see if an opponent piece might kill my agent's piece
                                    {
                                        myState.Pieces[index] += score;

                                        return;
                                    }

                                }

                            }

                        }
                    }

                    if (myState.Pieces[index] + score == 8 || myState.Pieces[index] + score == 13 || myState.Pieces[index] + score == 21 || myState.Pieces[index] + score == 26 || myState.Pieces[index] + score == 34 || myState.Pieces[index] + score == 39 || myState.Pieces[index] + score == 47 || myState.Pieces[index] + score > 50)
                    {
                        myState.Pieces[index] += score;

                        return;
                    }
                }

            }//end of for

            var r = rnd.Next(0, 4); //random piece to move

            for (int p2 = 0; p2 < 9; p2++) // loop for my agent's pieces
            {
                var index2 = (p2 + r) % 4;

                if (myState.Pieces[index2] > -1 && myState.Pieces[index2] < 56 && (myState.Pieces[index2] + score) <= 56)
                {
                    myState.Pieces[index2] += score;

                    return;
                }
            }

        }//end of MovePieceInBoard


        

    }//end of AgentManger

    class AgentState
    {
        public int AgentId;
        public int[] Pieces = new int[4] { -1, -1, -1, -1 };
        public AgentState()
        {
        }
        public AgentState(String state)
        {
            SetState(state);
        }
        public String GetState()
        {
            return String.Format("{0} {1} {2} {3}", Pieces[0], Pieces[1], Pieces[2], Pieces[3]);
        }
        public void SetState(String state)
        {
            var stArray = state.Split(" ".ToCharArray());

            for (int i = 0; i < 4; i++)
            {
                this.Pieces[i] = int.Parse(stArray[i]);
            }
        }
    }//end of AgentState
}
