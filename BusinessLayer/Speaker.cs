using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLayer
{
    /// <summary>
    /// Represents a single speaker
    /// </summary>
    public class Speaker
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Exp { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<BusinessLayer.Session> Sessions { get; set; }

        /// <summary>
        /// Register a speaker
        /// </summary>
        /// <returns>speakerID</returns>
        public int? Register(IRepository repository)
        {
            //lets init some vars
            int? speakerId = null;
            bool isGood = false;
            bool isAppr = false;
            //var nt = new List<string> {"MVC4", "Node.js", "CouchDB", "KendoUI", "Dapper", "Angular"};
            var ot = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

            //DEFECT #5274 DA 12/10/2012
            //We weren't filtering out the prodigy domain so I added it.
            var domains = new List<string>() { "aol.com", "hotmail.com", "prodigy.com", "CompuServe.com" };
            
            if (string.IsNullOrWhiteSpace(FirstName)) throw new ArgumentNullException("First Name is required");
            if (string.IsNullOrWhiteSpace(LastName)) throw new ArgumentNullException("Last name is required.");
            if (string.IsNullOrWhiteSpace(Email)) throw new ArgumentNullException("Email is required.");
            else
            {
                //put list of employers in array
                var emps = new List<string>() { "Microsoft", "Google", "Fog Creek Software", "37Signals" };

                //DFCT #838 Jimmy 
                //We're now requiring 3 certifications so I changed the hard coded number. Boy, programming is hard.
                isGood = ((Exp > 10 || HasBlog || Certifications.Count() > 3 || emps.Contains(Employer)));

                if (isGood)
                {
                    if (Sessions.Count() == 0) throw new ArgumentException("Can't register speaker with no sessions to present.");
                    else
                    {
                        foreach (var session in Sessions)
                        {
                            foreach (var tech in ot)
                            {
                                if (session.Title.Contains(tech) || session.Description.Contains(tech))
                                {
                                    session.Approved = false;
                                    break;
                                }
                                else
                                {
                                    session.Approved = true;
                                    isAppr = true;
                                }
                            }
                        }
                    }
                    if (!isAppr) throw new NoSessionsApprovedException("No sessions approved.");

                    if (Sessions.Count() != 0 && isAppr)
                    {                        

                        //if we got this far, the speaker is approved
                        //let's go ahead and register him/her now.
                        //First, let's calculate the registration fee. 
                        //More experienced speakers pay a lower fee.
                        if (Exp <= 1)
                        {
                            RegistrationFee = 500;
                        }
                        else if (Exp >= 2 && Exp <= 3)
                        {
                            RegistrationFee = 250;
                        }
                        else if (Exp >= 4 && Exp <= 5)
                        {
                            RegistrationFee = 100;
                        }
                        else if (Exp >= 6 && Exp <= 9)
                        {
                            RegistrationFee = 50;
                        }
                        else
                        {
                            RegistrationFee = 0;
                        }

                        //Now, save the speaker and sessions to the db.
                        try
                        {
                            speakerId = repository.SaveSpeaker(this);
                        }
                        catch (Exception e)
                        {
                            //in case the db call fails 
                        }
                    }
                }
                else
                {
                    string emailDomain = Email.Split('@').Last();
                    isGood = (!domains.Contains(emailDomain) && (!(Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)));
                    throw new SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.");
                }
            }


            //if we got this far, the speaker is registered.
            return speakerId;
        }

        #region Custom Exceptions
        public class SpeakerDoesntMeetRequirementsException : Exception
        {
            public SpeakerDoesntMeetRequirementsException(string message)
                : base(message)
            {
            }

            public SpeakerDoesntMeetRequirementsException(string format, params object[] args)
                : base(string.Format(format, args)) { }
        }

        public class NoSessionsApprovedException : Exception
        {
            public NoSessionsApprovedException(string message)
                : base(message)
            {
            }
        }
        #endregion
    }
}