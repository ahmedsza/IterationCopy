using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {


 
            string tfsServer = args[0].ToString();
            string tfsProject = args[1].ToString();







            string azDoServer = args[2].ToString();
            string PAT = args[3].ToString();
            string azDoProject = args[4].ToString();


            Console.WriteLine(tfsServer);
            Console.WriteLine(tfsProject);
            Console.WriteLine(azDoProject);
            Console.WriteLine(PAT);
            Console.WriteLine(azDoProject);

            VssConnection azDoconnection = new VssConnection(new Uri(azDoServer), new VssBasicCredential(string.Empty, PAT));
            var azDoworkItemTracking = azDoconnection.GetClient<WorkItemTrackingHttpClient>();
            var azdoNode = azDoworkItemTracking.GetClassificationNodeAsync(project: azDoProject, structureGroup: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.TreeStructureGroup.Iterations, depth: 5).Result;
            // PrintNodeInfo(iteration);
            DeleteNodes(azDoconnection, azDoProject, azDoProject, azdoNode);



            //  string username = "";
            // string pwd = "";
            //NetworkCredential networkCredential = new NetworkCredential(username, pwd);
            //Microsoft.VisualStudio.Services.Common.WindowsCredential winCred = new Microsoft.VisualStudio.Services.Common.WindowsCredential(networkCredential);
            //VssCredentials vssCred = new VssClientCredentials(winCred);
            //VssConnection connection = new VssConnection(new Uri(tfsServer), vssCred);

            VssConnection connection = new VssConnection(new Uri(tfsServer), new VssCredentials());

            var workItemTracking = connection.GetClient<WorkItemTrackingHttpClient>();
            Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient projClient = connection.GetClient<Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient>();
            var iteration = workItemTracking.GetClassificationNodeAsync(project: tfsProject, structureGroup: Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.TreeStructureGroup.Iterations, depth: 5).Result;
            //   PrintNodeInfo(iteration);
            CopyNodeInfo(azDoconnection, azDoProject, azDoProject, iteration, azDoProject);


            //   WorkItemClassificationNode newNode = CreateArea(azDoconnection,azDoProject, @"Application" ); //Add area Application

        }


        static WorkItemClassificationNode CreateArea(VssConnection conn, string TeamProjectName, string AreaName, string ParentAreaPath = null)
        {
            WorkItemClassificationNode newArea = new WorkItemClassificationNode();
            newArea.Name = AreaName;

            return conn.GetClient<WorkItemTrackingHttpClient>().CreateOrUpdateClassificationNodeAsync(newArea, TeamProjectName, TreeStructureGroup.Areas, ParentAreaPath).Result;
        }


        static WorkItemClassificationNode CreateIteration(VssConnection conn, string TeamProjectName, string IterationName, DateTime? StartDate = null, DateTime? FinishDate = null, string ParentIterationPath = null)
        {
            Console.WriteLine("creating iteration:" + IterationName);
            WorkItemClassificationNode newIteration = new WorkItemClassificationNode();
            newIteration.Name = IterationName;

            if (StartDate != null && FinishDate != null)
            {
                newIteration.Attributes = new Dictionary<string, object>();
                newIteration.Attributes.Add("startDate", StartDate);
                newIteration.Attributes.Add("finishDate", FinishDate);
            }

            return conn.GetClient<WorkItemTrackingHttpClient>().CreateOrUpdateClassificationNodeAsync(newIteration, TeamProjectName, TreeStructureGroup.Iterations, ParentIterationPath).Result;
        }


        static void GetIterations(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItemClassificationNode currentIteration)
        {
            Console.WriteLine(currentIteration.Url);
            //    Console.WriteLine(currentIteration.Attributes["startDate"].ToString());
            //    Console.WriteLine(currentIteration.Attributes["endDate"].ToString());

            if (currentIteration.Attributes != null)
            {
                foreach (var item in currentIteration.Attributes)
                {
                    Console.WriteLine(item.Key + ":" + item.Value);
                }
            }
            if (currentIteration.Children != null)
            {
                foreach (var ci in currentIteration.Children)
                {
                    GetIterations(ci);
                }
            }
        }





        static void PrintNodeInfo(WorkItemClassificationNode Node)
        {
            Console.WriteLine("{0} name: {1}", (Node.StructureType == TreeNodeStructureType.Area) ? "Area" : "Iteration", Node.Name);

            //get path from url
            string[] pathArray = Node.Url.Split(new string[] { (Node.StructureType == TreeNodeStructureType.Area) ? "/Areas/" : "/Iterations/" },
                StringSplitOptions.RemoveEmptyEntries);
            if (pathArray.Length == 2) Console.WriteLine("Path: " + pathArray[1].Replace('/', '\\'));

            if (Node.Attributes != null)
            {
                Console.WriteLine("Start Date: {0}", (Node.Attributes.ContainsKey("startDate")) ? Node.Attributes["startDate"].ToString() : "none");
                Console.WriteLine("Finish Date: {0}", (Node.Attributes.ContainsKey("finishDate")) ? Node.Attributes["finishDate"].ToString() : "none");
            }

            if (Node.Children != null)
            {
                foreach (var ci in Node.Children)
                {
                    PrintNodeInfo(ci);
                }
            }
        }

        static void CopyNodeInfo(VssConnection conn, string TeamProjectName, string IterationName, WorkItemClassificationNode Node, string targetIterationName)
        {



            if (Node.Children != null)
            {
                foreach (var ci in Node.Children)
                {
                    if (Node.Id == 1)
                    {
                        if (ci.Attributes != null)
                        {
                            WorkItemClassificationNode newNode1 = CreateIteration(conn, TeamProjectName, ci.Name, Convert.ToDateTime(ci.Attributes["startDate"]), Convert.ToDateTime(ci.Attributes["finishDate"])); //Add iteraion R2
                        }
                        else
                        {
                            WorkItemClassificationNode newNode1 = CreateIteration(conn, TeamProjectName, ci.Name);

                        }
                    }
                    else
                    {
                        if (ci.Attributes!=null)
                        {
                            WorkItemClassificationNode newNode1 = CreateIteration(conn, TeamProjectName, ci.Name, Convert.ToDateTime(ci.Attributes["startDate"]), Convert.ToDateTime(ci.Attributes["finishDate"]), targetIterationName); //Add iteraion R2
                        }
                        else
                        {

                            string[] pathArray = Node.Url.Split(new string[] { (Node.StructureType == TreeNodeStructureType.Area) ? "/Areas/" : "/Iterations/" },
             StringSplitOptions.RemoveEmptyEntries);
                            if (pathArray.Length == 2) Console.WriteLine("Path: " + pathArray[1].Replace('/', '\\'));
                            string temppath = pathArray[1].Replace('/', '\\');
                            WorkItemClassificationNode newNode1 = CreateIteration(conn, TeamProjectName, ci.Name,ParentIterationPath: temppath);

                        }
                    }

                    CopyNodeInfo(conn, TeamProjectName, IterationName, ci, ci.Name);
                }
            }







        }

        static void DeleteNodes(VssConnection conn, string TeamProjectName, string IterationName, WorkItemClassificationNode Node)
        {
            Console.WriteLine("deleting node:" + Node.Name);
            if (Node.Children != null)
            {
                foreach (var ci in Node.Children)
                {
                    conn.GetClient<WorkItemTrackingHttpClient>().DeleteClassificationNodeAsync(TeamProjectName, TreeStructureGroup.Iterations, ci.Name).SyncResult();

                }
            }
        }

    }
     
}
