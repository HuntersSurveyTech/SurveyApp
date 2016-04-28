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


    public enum ESurveyorType
    {
        Common = 0,
        QA = 1
    }



    public class ApiResponse<T>
    {
        public Exception Exception { get; set; }


        public T Data { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }



    public class Entity
    {
        public Entity()
        {
            Id = Guid.NewGuid();
            AppVersion = Helpers.GetAppVersion();
        }


        [PrimaryKey]
        public Guid Id { get; set; }

        [Indexed]
        public byte SyncStatus { get; set; }
        //public Guid? SyncErrorId { get; set; }

        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        
        public bool IsCreatedOnClient { get; set; }
        public int NetmeraReportTimestamp { get; set; }
        public string AppVersion { get; set; }

        public bool IsDeletedOnClient { get; set; }

       // public DateTime? UpdateDate { get; set; }

    }


    public class Customer : Entity
    {
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerShortName { get; set; }
        public string CustomerSurveyID { get; set; }
        public string SurveyImportDate { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
        public string Surveyors { get; set; }

    }


    public class Surveyor : Entity
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string SurveyorName { get; set; }
        public string Choose { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }

        public int SyncTimestampForQuestions { get; set; }

        public int Type { get; set; }
    }

    //public class AddressMove : Entity
    //{
    //    public Guid AddressId { get; set; }
    //    public Guid? FromSurveyor { get; set; }
    //    public Guid? ToSurveyor { get; set; }
    //}

    public class AddressStatus : Entity
    {
        public Guid AddressId { get; set; }
        public int CompletedQuestionsCount { get; set; }
        public bool IsCompleted { get; set; }
    }


    public class Address : Entity
    {
        public string UPRN { get; set; }

        public string Type { get; set; }

        public bool TypeUpdated { get; set; }

        public Guid SurveyorId { get; set; }
        public string FullAddress { get; set; }

        public string DateSurveyed { get; set; }

        public bool CopyTo { get; set; }


        public bool AllowCopyFrom { get; set; }
        public string CopiedFrom { get; set; }
        public string AddressLine1 { get; set; }


        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }


        [Ignore]
        public bool IsSelected { get; set; }

        public bool IsCompleted { get; set; }

        public bool HasStartedToSurveyed { get; set; }

        public bool RemoveDataAfterSync { get; set; }

        public bool IsAlreadyCheckedPropertyType { get; set; }


        public bool IsLoadToPhone { get; set; }

    }



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

        public bool createPDF { get; set; }

        public bool isDate { get; set; }

        public bool NeedToHaveMedia { get; set; }

    }


    public class Option : Entity
    {
       
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

        public bool createPDF { get; set; }

        public bool NeedToHaveMedia { get; set; }

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


    public class RichMedia : Entity
    {
        [Indexed]
        public string UPRN { get; set; }
        public string Comments { get; set; }
        public string CloudPath { get; set; }
        public string FileName { get; set; }

        [Indexed]
        public string Question_Ref { get; set; }


        public Guid? Option_ID { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

        public string PDFFileName { get; set; }

        public bool IsCreatePDF { get; set; }

        public string WatermarkText { get; set; }

    }


    public class Survelem : Entity
    {

        public Guid? OptionID { get; set; }
        public Guid? OptionID2ndry { get; set; }

        [Indexed]
        public string UPRN { get; set; }

        [Indexed]
        public string Question_Ref { get; set; }
        public string COMMENT { get; set; }
        public string Freetext { get; set; }
        public string BuildingType { get; set; }
        public string DateOfSurvey { get; set; }
   

        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

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

        public bool  RemoveDataAfterSync { get; set; }
    }



    public class SurveyType : Entity
    {
      
        public string Name { get; set; }

        public string Value { get; set; }

        public long CustomerID { get; set; }

        public string CustomerSurveyID { get; set; }
    }


    public class SurvelemMap : Entity
    {
        
        public string Question_Ref { get; set; }
        public string SqName { get; set; }
        public string Question_Heading { get; set; }
        public string Formats { get; set; }
        public string SurveyType { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

        public int Order { get; set; }
    }



    //public class QuestionStatus:Entity
    //{
    //    public bool Completed { get; set; }
    //    public string QuestionRef { get; set; }
    //    public string UPRN { get; set; }
    //}

    public class QAAddress : Entity
    {
        public Guid AddressId { get; set; }

        public Guid SurveyorId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class QAAddressComment : Entity
    {
        public Guid AddressId { get; set; }

        public string QuestionRef { get; set; }
        public string Text { get; set; }
    }

    public class AddressQuestionGroupStatus : Entity
    {
        public Guid AddressId { get; set; }

        public string Group { get; set; }

        public bool IsCompleted { get; set; }
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
        public bool IsCompleted { get; set; }

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
