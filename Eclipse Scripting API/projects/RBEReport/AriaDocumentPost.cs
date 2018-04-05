////////////////////////////////////////////////////////////////////////////////
//  
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VMS.ARIA.DocumentService.WebService.Entities;
using VMS.ARIA.DocumentService.WebService.WebClient;
using VMS.ARIA.REST.Shared.WebClientBase;

namespace RBEReport
{
  class AriaDocumentPost
  {
    internal static string SERVICE_URL = "https://172.20.168.185:56001/DocumentService";
    internal static string USERID = "SysAdmin";
    internal static string PASSWORD = "SysAdmin";

    private static DocumentService CreateService()
    {
      // Causes this application to trust all SSL certificates. The Document Service is installed with a self-signed
      // server certificate by default which means that, from a client application's perspective, the certificate was not
      // issued by a trusted certificate authority. For most applications running within a LAN this is not a big concern  
      // and it is recommended that in this case server certificate validation be ignored. Please see the User Guide for 
      // alternative options, such as adding the server as a trusted certification authority or installing the client
      // certificate onto client machines.
      System.Net.ServicePointManager.ServerCertificateValidationCallback = (
          (sender, certificate, chain, sslPolicyErrors) => true
      );

      // Creates a web client for the document service:
      return new DocumentService(SERVICE_URL, USERID, PASSWORD);
    }


    public static void Post(string documentFile, string patientId)
    {
      //string temp = System.Environment.GetEnvironmentVariable("TEMP");
      //string command = string.Format(CMD_FILE_FMT, DCMTK_BIN_PATH, URL, USER, PWD, PARAMFILE, documentFile);

      Document document = null;
      byte[] documentContents;
      using (FileStream fs = File.OpenRead(documentFile))
      {
        documentContents = new byte[fs.Length];
        int length = (int)fs.Length;
        fs.Read(documentContents, 0, length);
      }

      var service = CreateService();

      string patientId1Value = "#" + patientId; // Use # for an id1 value, $ for a varianenm database pt_id value, or ~ for a variansystem database PatientSer value.

      try
      {
        document = service.InsertDocument(new InsertDocumentParameters()
        {
          PatientId = patientId1Value,
          BinaryContentAsBytes = documentContents,
          FileFormat = "PDF",
          DateOfService = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")

        });
      }
      catch (WebClientBaseException ex)
      {
        Console.WriteLine(ex.Message);

        // Example of handling a known service error:
        if (ex.ServiceError != null && ex.ServiceError.ErrorCode == "ECB_000")
        {
          Console.WriteLine("The login credentials are not correct. Please check them and try again.");
        }

        throw;
      }

      //return document;

    }
  }
}
