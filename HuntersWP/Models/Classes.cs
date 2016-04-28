using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HuntersWP.Services;
using Newtonsoft.Json;
using SQLite;

namespace HuntersWP.Models
{
    public enum ESyncStatus
    {
        NotSynced,
        InProcess,
        Error,
        Success
    }

    public enum EQuestionGroupStatus
    {
        All,
        Incomplete,
        Complete
    }

    public enum EAddressStatus
    {
        All,
        ToSurvey,
          InComplete,
        Surveyed
      
    }



    public class ApiResponse<T>
    {
        public Exception Exception { get; set; }


        public T Data { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class Id
    {
        public int inc { get; set; }
        public int machine { get; set; }
        public bool @new { get; set; }
        public long time { get; set; }
        public int timeSecond { get; set; }
    }

    public class Entity
    {
        public Entity()
        {
            
        }

        public string parentPath { get; set; }

        public string contentPrivacy { get; set; }

        [Ignore]
        public Id _id { get; set; }

        public byte SyncStatus { get; set; }
        public string SyncErrorId { get; set; }

        [PrimaryKey]
        public string Identity { get; set; }

        public bool IsCreatedOnClient { get; set; }


    }

    /// <summary>
    /// OK
    /// </summary>
    public class Customer : Entity
    {
        public string ID { get; set; }
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerShortName { get; set; }
        public string CustomerSurveyID { get; set; }
        public string SurveyImportDate { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
        public string Surveyors { get; set; }

    }

    /// <summary>
    /// OK
    /// </summary>

    public class Surveyor : Entity
    {
        public long id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        [JsonProperty("Surveyor name")]
        public string SurveyorName { get; set; }
        public string Choose { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    /// <summary>
    /// OK
    /// </summary>
    public class Address : Entity
    {
        public bool PTUpdated { get; set; }
        public string AddressID { get; set; }
        public string UPRN { get; set; }
        public string BlockUPRN { get; set; }
        public string FlatNo { get; set; }
        public string BuildingName { get; set; }
        public string StreetNo { get; set; }
        public string StreetName { get; set; }
        public string Postcode { get; set; }
        public string Type { get; set; }
        public string QuestionGrp { get; set; }
        public string Bedrooms { get; set; }
        public string Surveyor { get; set; }
        public string FullAddress { get; set; }
        public string Multipliers { get; set; }
        public string DateSurveyed { get; set; }
        public string SAPRating { get; set; }
        public string SAPBand { get; set; }
        public string LeaseHolderAddress { get; set; }
        public string Floor { get; set; }
        public bool CopyTo { get; set; }
        public bool Complete { get; set; }
        public string Visited { get; set; }
        public string Submit { get; set; }
        public string Submitted { get; set; }
        public bool AllowCopyFrom { get; set; }
        public string CopiedFrom { get; set; }
        [JsonProperty("Address line 1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("Address line 2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("Address line 3")]
        public string AddressLine3 { get; set; }

        [JsonProperty("Address line 4")]
        public string AddressLine4 { get; set; }

        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }


        [Ignore]
        public bool IsSelected { get; set; }

        public bool HasStartedToSurveyed { get; set; }

        //[Ignore]
        //public bool IsCompleted
        //{
        //    get
        //    {
        //        return Complete.ToUpper() == "TRUE";
        //    }
        //}

    }


    /// <summary>
    /// ok
    /// </summary>
    public class Survey : Entity
    {
        public string id { get; set; }
        public string UPRN { get; set; }
        public string QuestionsAsked { get; set; }
        public string QuestionsAnswered { get; set; }
        public string Question_Ref { get; set; }
        public string OptionID { get; set; }
        public string Choose { get; set; }
        public string CopiedFrom { get; set; }
    }

    /// <summary>
    /// ok
    /// </summary>
    public class Question : Entity
    {
        public int Question_Order { get; set; }
        public string Question_Ref { get; set; }
        public string Main_Element { get; set; }
        public string Question_Heading { get; set; }
        public string Category { get; set; }
        public string Question_Type { get; set; }
        public string Apply_2nd_Question { get; set; }
        public string Unit { get; set; }
        public string SurveyType { get; set; }
        [JsonProperty("Decent Homes")]
        public string DecenWtHomes { get; set; }
        public bool ExcludeFromClone { get; set; }
        public string QuestionGroup { get; set; }
        public string Display { get; set; }
        public string LeaseholderQuestion { get; set; }
        public string AnswerRangeFrom { get; set; }
        public string AnswerRangeTo { get; set; }
        public string AnswerRangeTextF { get; set; }
        public string AnswerRangeTextT { get; set; }
        public string LookAtRange { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
        public bool finishSection { get; set; }

    }

    /// <summary>
    /// OK
    /// </summary>
    public class Option : Entity
    {
        public string OptionId { get; set; }
        public string Question_Ref { get; set; }
        public string Level { get; set; }
        public int Option_Number { get; set; }
        public string Text { get; set; }
        public string Jumpto { get; set; }
        public bool Choose { get; set; }
        public bool ConfirmationRequired { get; set; }
        public string LifeGuide { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

        public bool Disable2nd { get; set; }

        [Ignore]
        public string DisplayText { get; set; }

        
        [Ignore]
        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(DisplayText)) return DisplayText;

                if (string.IsNullOrEmpty(LifeGuide)) return Text;

                return string.Format("{0} (Guide: {1})",Text,LifeGuide);
            }
        }
    }

    /// <summary>
    /// ok
    /// </summary>
    public class RichMedia : Entity
    {
        public string ID { get; set; }
        public string UPRN { get; set; }
        public string Comments { get; set; }
        public string FileName { get; set; }
        public string Question_Ref { get; set; }
        public string Option_ID { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
    }

    /// <summary>
    /// ok
    /// </summary>
    public class Survelem : Entity
    {
        public string id { get; set; }
        public string OptionID { get; set; }
        public string OptionID2ndry { get; set; }
        public string UPRN { get; set; }
        public string Question_Ref { get; set; }
        public string COMMENT { get; set; }
        public string Freetext { get; set; }
        public string BuildingType { get; set; }
        public string DateOfSurvey { get; set; }
        public string SqT1 { get; set; }
        public string SqN1 { get; set; }
        public string SqT2 { get; set; }
        public string SqN2 { get; set; }
        public string SqT3 { get; set; }
        public string SqN3 { get; set; }
        public string SqT4 { get; set; }
        public string SqN4 { get; set; }
        public string SqT5 { get; set; }
        public string SqN5 { get; set; }
        public string SqT6 { get; set; }
        public string SqN6 { get; set; }
        public string SqT7 { get; set; }
        public string SqN7 { get; set; }
        public string SqT8 { get; set; }
        public string SqN8 { get; set; }
        public string SpotPriceFF { get; set; }
        public string SqT9 { get; set; }
        public string SqN9 { get; set; }
        public string SqT10 { get; set; }
        public string SqN10 { get; set; }
        public string SqT11 { get; set; }
        public string SqT12 { get; set; }
        public string SqT13 { get; set; }
        public string SqT14 { get; set; }
        public string SqT15 { get; set; }

        public string SqN11 { get; set; }
        public string SqN12 { get; set; }
        public string SqN13 { get; set; }
        public string SqN14 { get; set; }
        public string SqN15 { get; set; }

        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
    }

    /// <summary>
    /// OK
    /// </summary>
    public class SurveyTypes : Entity
    {
        public string ID { get; set; }
        public string Name { get; set; }
        [JsonProperty("SurveyTypes")]
        public string Value { get; set; }
    }

    /// <summary>
    /// OK
    /// </summary>
    public class SurvelemMap : Entity
    {
        public string SvmMapID { get; set; }
        public string Question_Ref { get; set; }
        public string SqName { get; set; }
        public string Question_Heading { get; set; }
        public string Formats { get; set; }
        public string SurveyType { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
    }


    ///// <summary>
    ///// OK
    ///// </summary>
    //public class NetmeraUser : Entity
    //{
    //    public string ID { get; set; }
    //    public string CustomerID { get; set; }
    //    public string CustomerName { get; set; }
    //    public string CustomerShortName { get; set; }
    //    public string CustomerSurveyID { get; set; }
    //    public string SurveyImportDate { get; set; }
    //    public string Notes { get; set; }

    //}

    public class QuestionStatus:Entity
    {
        public bool Completed { get; set; }
        public string QuestionRef { get; set; }
        public string UPRN { get; set; }
    }

    public class SyncError : Entity
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
    }



    public class BaseObject : INotifyPropertyChanged
    {
        public BaseObject()
        {
            IsNotifying = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        bool isNotifying;
        public bool IsNotifying
        {
            get
            {
                return isNotifying;
            }
            set
            {
                isNotifying = value;
            }
        }


        public virtual void NotifyOfPropertyChange(string propertyName)
        {
            if (IsNotifying)
            {

                OnPropertyChanged(propertyName);

            }
        }

        public virtual void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            NotifyOfPropertyChange(property.GetMemberInfo().Name);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.NotifyOfPropertyChange(propertyName);
            return true;
        }

    }

    public class QuestionGroup :BaseObject
    {
        public string Name { get; set; }
        public bool Complete { get; set; }

        public List<Question> Questions { get; set; } 
    }

    public class TextValueItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public TextValueItem(string text, object value)
        {
            Text = text;
            Value = value;
        }
    }
}
