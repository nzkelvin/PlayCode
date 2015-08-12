using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NZKelvin.Config.ConfigDefinition
{
    /// <summary>
    /// {
    /// "RefereeValidationRules":
    ///     [
    ///         {"NumOfRefereesRequired":1,"ScholarshipTypeName":"Excellence","ScholarshipTypeId":183870000},
    ///         {"NumOfRefereesRequired":2,"ScholarshipTypeName":"FutureLeader","ScholarshipTypeId":183870001},
    ///         {"NumOfRefereesRequired":2,"ScholarshipTypeName":"GlobalChallenge","ScholarshipTypeId":183870002},
    ///         {"NumOfRefereesRequired":2,"ScholarshipTypeName":"Sport","ScholarshipTypeId":183870003}
    ///     ],
    /// "ContactRequiredFieldRules":
    ///     [
    ///         {"FieldName":"koo_lastschoolyear","FieldLabel":"Last School Year","ContactTypeName":"Student","ContactTypeId":121590000}
    ///     ]
    /// }

    /// </summary>
    [DataContract]
    public class ApplicationValidationRules
    {
        [DataMember]
        public RefereeValidationRule[] RefereeValidationRules;

        [DataMember]
        public ContactRequiredFieldRule[] ContactRequiredFieldRules;
    }

    [DataContract]
    public class RefereeValidationRule
    {
        [DataMember]
        public int NumOfRefereesRequired;
        [DataMember]
        public string ScholarshipTypeName;
        [DataMember]
        public int ScholarshipTypeId;
    }

    [DataContract]
    public class ContactRequiredFieldRule
    {
        [DataMember]
        public string ContactTypeName;

        [DataMember]
        public int ContactTypeId;

        [DataMember]
        public string FieldName;

        [DataMember]
        public string FieldLabel;
    }
}
