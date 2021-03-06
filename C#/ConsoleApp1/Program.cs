﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Office.Interop.Outlook;
using System.IO;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {

        // Path where attachments will be saved
        static string basePath = @"c:\temp\emails";
        static int totalfilesize = 0;

        static void Main(string[] args)
        {
            EnumerateAccounts();
            EnumerateFoldersInDefaultStore();
           
        }

        static void EnumerateFoldersInDefaultStore()
        {
            Microsoft.Office.Interop.Outlook.Application Application = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.Folder root = Application.Session.DefaultStore.GetRootFolder() as Microsoft.Office.Interop.Outlook.Folder;
            EnumerateFolders(root);
        }

        // Uses recursion to enumerate Outlook subfolders.
        static void EnumerateFolders(Microsoft.Office.Interop.Outlook.Folder folder)
        {
            Microsoft.Office.Interop.Outlook.Folders childFolders = folder.Folders;
            if (childFolders.Count > 0)
            {
                foreach (Microsoft.Office.Interop.Outlook.Folder childFolder in childFolders)
                {
                    // We only want Inbox folders - ignore Contacts and others
                    if (childFolder.FolderPath.Contains("DalkanimPrint"))
                    {
                        
                        // Call EnumerateFolders using childFolder, to see if there are any sub-folders within this one
                        EnumerateFolders(childFolder);
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Checking in.... " + folder.FolderPath);
            Console.ForegroundColor = ConsoleColor.White;
            IterateMessages(folder);
        }

        static void IterateMessages(Microsoft.Office.Interop.Outlook.Folder folder)
        {
            // attachment extensions to save
            string[] extensionsArray = {".docx", "xlsx", ".pdf", ".doc"};

            // Iterate through all items ("messages") in a folder
            var fi = folder.Items;
            if (fi != null)
            {
                

                try
                {
                    foreach (Object item in fi)
                    {
                       
                        Microsoft.Office.Interop.Outlook.MailItem mi = (Microsoft.Office.Interop.Outlook.MailItem)item;
                        var attachments = mi.Attachments;

                        DateTime timeToChack = DateTime.Now;
                        
                        if (mi.CreationTime < timeToChack.AddMinutes(-5)) {

                            continue;
                        }
                        
                        

                        if (attachments.Count != 0)
                            {
                                //Console.WriteLine(mi.Sender.Address);
                                //Console.WriteLine(mi.Subject + " [" + attachments.Count + "]");
                                //Console.WriteLine(generateFolder(folder.FolderPath, mi.Sender.Address));
                                for (int i = 1; i <= mi.Attachments.Count; i++)
                                {
                                    var fn = mi.Attachments[i].FileName.ToLower();
                                    //check wither any of the strings in the extensionsArray are contained within the filename
                                    if (extensionsArray.Any(fn.Contains))
                                    {

                                        // Create a further sub-folder for the sender
                                        if (!Directory.Exists(basePath))
                                        {

                                            Directory.CreateDirectory(basePath);
                                        }
                                        totalfilesize = totalfilesize + mi.Attachments[i].Size;
                                        if (!File.Exists(basePath + mi.Attachments[i].FileName))
                                        {
                                            Console.WriteLine("Saving " + mi.Attachments[i].FileName);
                                            mi.Attachments[i].SaveAsFile(basePath + mi.Attachments[i].FileName);



                                            ProcessStartInfo info = new ProcessStartInfo();
                                            info.Verb = "print";
                                            info.FileName = basePath + mi.Attachments[i].FileName;
                                            info.CreateNoWindow = true;
                                            info.WindowStyle = ProcessWindowStyle.Hidden;

                                            Process p = new Process();
                                            p.StartInfo = info;
                                            p.Start();

                                            p.WaitForInputIdle();
                                            System.Threading.Thread.Sleep(3000);
                                            if (false == p.CloseMainWindow())
                                                p.Kill();

                                            //mi.Attachments[i].Delete();
                                        }
                                        else
                                        {

                                            Console.WriteLine("Already saved " + mi.Attachments[i].PropertyAccessor);
                                        }
                                    }
                                }
                            }
                    }
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("An error occurred: '{0}'", e);
                }
            }
        }

        // Retrieves the email address for a given account object
        static string EnumerateAccountEmailAddress(Microsoft.Office.Interop.Outlook.Account account)
        {
            try
            {
                if (string.IsNullOrEmpty(account.SmtpAddress) || string.IsNullOrEmpty(account.UserName))
                {
                    Microsoft.Office.Interop.Outlook.AddressEntry oAE = account.CurrentUser.AddressEntry as Microsoft.Office.Interop.Outlook.AddressEntry;
                    if (oAE.Type == "EX")
                    {
                        Microsoft.Office.Interop.Outlook.ExchangeUser oEU = oAE.GetExchangeUser() as Microsoft.Office.Interop.Outlook.ExchangeUser;
                        return oEU.PrimarySmtpAddress;
                    }
                    else
                    {
                        return oAE.Address;
                    }
                }
                else
                {
                    return account.SmtpAddress;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        static void EnumerateAccounts()
        {
            Console.Clear();
            
            Microsoft.Office.Interop.Outlook.Application Application = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.Accounts accounts = Application.Session.Accounts;
            
            while (true == true)
            {
                
                foreach (Account account in accounts)
                {
                    //moti@u-btech.com
                    //alondi@sonol.co.il
                    if (account.DisplayName == "maildalkanim@sonol.co.il")
                    {
                        int id = accounts.Count;
                        Microsoft.Office.Interop.Outlook.Folder selectedFolder = Application.Session.DefaultStore.GetRootFolder() as Microsoft.Office.Interop.Outlook.Folder;
                        selectedFolder = GetFolder(@"\\" + accounts[id].DisplayName);
                        EnumerateFolders(selectedFolder);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n" + "- Refrashing in 10 secondes.....");
                        Console.ForegroundColor = ConsoleColor.White;
                        System.Threading.Thread.Sleep(300000);
                        
                    }
                    
                   }
                
                }
            }

        

        // Returns Folder object based on folder path
        static Microsoft.Office.Interop.Outlook.Folder GetFolder(string folderPath)
        {
            Console.WriteLine("Looking for: " + folderPath);
            Microsoft.Office.Interop.Outlook.Folder folder;
            string backslash = @"\";
            try
            {
                if (folderPath.StartsWith(@"\\"))
                {
                    folderPath = folderPath.Remove(0, 2);
                }
                String[] folders = folderPath.Split(backslash.ToCharArray());
                Microsoft.Office.Interop.Outlook.Application Application = new Microsoft.Office.Interop.Outlook.Application();
                folder = Application.Session.Folders[folders[0]] as Microsoft.Office.Interop.Outlook.Folder;
                if (folder != null)
                {
                    for (int i = 1; i <= folders.GetUpperBound(0); i++)
                    {
                        Microsoft.Office.Interop.Outlook.Folders subFolders = folder.Folders;
                        folder = subFolders[folders[i]] as Microsoft.Office.Interop.Outlook.Folder;
                        if (folder == null)
                        {
                            return null;
                        }
                    }
                }
                return folder;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }


}

