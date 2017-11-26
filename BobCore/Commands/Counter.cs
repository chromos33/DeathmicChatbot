using System;
using System.Collections.Generic;
using System.Linq;
using BobCore.DataClasses;

namespace BobCore.Commands
{
    class addCounter : IFCommand
    {
        public string[] Requirements = { "Counter" };
        public string sTrigger { get { return "!counter"; } }

        public string[] SARequirements
        {
            get { return Requirements; }
        }

        public bool @private { get { return false; } }

        public string category { get { return "Counter"; } }

        public string description { get { return "Dazu da einen Counter hochzuzählen"; } }

        public List<DataClasses.Counter> lCounter;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!counter [countername] ([number to add])";
                    }
                    else
                    {
                        string output = "";
                        if (lCounter.Where(x => x.name.ToLower() == @params[0].ToLower()).FirstOrDefault() == null)
                        {
                            lCounter.Add(new Counter(@params[0]));
                        }
                        if (@params.Count == 1)
                        {
                            output = "Counter " + @params[0].ToLower() + ": " + lCounter.Where(x => x.name.ToLower() == @params[0].ToLower()).FirstOrDefault().add();
                        }
                        if (@params.Count == 2)
                        {
                            try
                            {
                                output = "Counter " + @params[0].ToLower() + ": " + lCounter.Where(x => x.name.ToLower() == @params[0].ToLower()).FirstOrDefault().add(Int32.Parse(@params[1]));
                            }
                            catch (Exception)
                            {
                                output = "Error ([number to add]) must be a whole number";
                            }
                        }


                        BobCore.Administrative.XMLFileHandler.writeFile(lCounter, "Counters");
                        return output;
                    }
                }
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.Counter")
            {
                lCounter = _DataList;
            }
        }
    }
    class setCounter : IFCommand
    {
        private string[] Requirements = { "Counter" };
        public string category { get { return "Counter"; } }
        public string description { get { return "Dazu da einen Counter auf einen Wert zu setzen"; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!setcounter"; } }
        public bool @private { get { return false; } }
        public List<DataClasses.Counter> lCounter;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!setcounter [countername] [value]";
                    }
                    else
                    {
                        string output = "";

                        if (lCounter.Where(x => x.name == @params[0].ToLower()).FirstOrDefault() == null)
                        {
                            lCounter.Add(new Counter(@params[0]));
                        }
                        if (@params.Count == 2)
                        {
                            try
                            {
                                output = "Counter " + @params[0].ToLower() + ": " + lCounter.Where(x => x.name.ToLower() == @params[0].ToLower()).FirstOrDefault().set(Int32.Parse(@params[1]));
                            }
                            catch (Exception)
                            {
                                output = "Error: value must be a whole number";
                            }
                        }
                        else
                        {
                            output = "!setcounter [countername] [value]";
                        }


                        BobCore.Administrative.XMLFileHandler.writeFile(lCounter, "Counters");
                        return output;
                    }
                }
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.Counter")
            {
                lCounter = _DataList;
            }
        }
    }
    class readCounter : IFCommand
    {
        private string[] Requirements = { "Counter" };
        public string category { get { return "Counter"; } }
        public string description { get { return "Dazu da den Wert eines Counters anzufordern"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!readcounter"; } }
        public List<DataClasses.Counter> lCounter;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!readcounter [countername]";
                    }
                    else
                    {
                        string output = "";
                        if (@params.Count == 1)
                        {
                            try
                            {
                                output = "Counter " + @params[0].ToLower() + ": " + lCounter.Where(x => x.name.ToLower() == @params[0].ToLower()).FirstOrDefault().get();
                            }
                            catch (Exception)
                            {
                                output = "Error: counter does not exist";
                            }
                        }
                        else
                        {
                            output = "!readcounter [countername]";
                        }


                        BobCore.Administrative.XMLFileHandler.writeFile(lCounter, "Counters");
                        return output;
                    }
                }
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.Counter")
            {
                lCounter = _DataList;
            }
        }
    }
    class CounterList : IFCommand
    {
        private string[] Requirements = { "Counter" };
        public string description { get { return "Dazu da alle Counter aufzulisten"; } }
        public string category { get { return "Counter"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!counterlist"; } }
        public List<DataClasses.Counter> lCounter;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                string output = "";
                foreach (Counter counter in lCounter)
                {
                    output += counter.name + ": " + counter.count + Environment.NewLine;
                }
                return output;
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.Counter")
            {
                lCounter = _DataList;
            }
        }
    }
}
