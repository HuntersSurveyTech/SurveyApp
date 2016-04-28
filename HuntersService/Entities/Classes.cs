using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace HuntersService.Entities
{
    public class MyDbContext : DbContext
    {
        public MyDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Surveyor> Surveyors { get; set; }
        public DbSet<SurveyType> SurveyTypes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<RichMedia> RichMedias { get; set; }
        public DbSet<SurvelemMap> SurvelemMaps { get; set; }
        public DbSet<Survelem> Survelems { get; set; }
        public DbSet<LogItem> LogItems { get; set; }
        public DbSet<AddressStatus> AddressStatus { get; set; }
        public DbSet<AddressMove> AddressMoves { get; set; }

        public DbSet<QAAddress> QAAddresses { get; set; }
        public DbSet<QAAddressComment> QAAddressComments { get; set; }

        public DbSet<AddressQuestionGroupStatus> AddressQuestionGroupStatuses { get; set; }

        
        
    }

    public enum ELogItemType
    {
        Exception
    }

    public enum ESurveyorType
    {
        Common = 0,
        QA = 1
    }



    public class Entity
    {
        public Entity()
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
        }

        [StringLength(100)]
        public string NetmeraId { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public Guid Id { get; set; }
        public Guid? LogItemId { get; set; }

        [StringLength(10)]
        public string AppVersion { get; set; }

        [NotMapped]
        public bool IsDeletedOnClient { get; set; }



    }

    [MessageContract]
    public class Customer : Entity
    {
        public long CustomerID { get; set; }

        [StringLength(200)]
        public string CustomerName { get; set; }

        [StringLength(200)]
        public string CustomerShortName { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }

        [StringLength(100)]
        public string SurveyImportDate { get; set; }

        [StringLength(400)]
        public string Notes { get; set; }

        public bool Completed { get; set; }

        [StringLength(200)]
        public string Surveyors { get; set; }

    }



    public class Surveyor : Entity
    {
        [StringLength(100)]
        public string first_name { get; set; }

        [StringLength(100)]
        public string last_name { get; set; }

        [StringLength(100)]
        public string SurveyorName { get; set; }

        [StringLength(100)]
        public string Choose { get; set; }

        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(100)]
        public string Username { get; set; }

        //increment field, to identify changes in questions/options
        public int SyncTimestampForQuestionsOptions { get; set; }

        public int Type { get; set; }
    }


    /// <summary>
    /// IF ADDREsS ADDED OR MOVED TO SURVEYOYR
    /// </summary>
    public class AddressMove : Entity
    {
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [ForeignKey("FromSurveyor")]
        public Guid? FromSurveyorId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("FromSurveyorId")]
        public Surveyor FromSurveyor { get; set; }

        [ForeignKey("ToSurveyor")]
        public Guid? ToSurveyorId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("ToSurveyorId")]
        public Surveyor ToSurveyor { get; set; }

        public bool IsProcessedFrom { get; set; }
        public bool IsProcessedTo { get; set; }
    }


    /// <summary>
    /// Adddres status for reports
    /// </summary>
    public class AddressStatus : Entity
    {
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        public int CompletedQuestionsCount { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class Address : Entity
    {
        [StringLength(100)]
        public string UPRN { get; set; }

        [StringLength(100)]
        public string Type { get; set; }

        [ForeignKey("Surveyor")]
        public Guid SurveyorId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("SurveyorId")]
        public Surveyor Surveyor { get; set; }

        [StringLength(200)]
        public string FullAddress { get; set; }

        [StringLength(100)]
        public string DateSurveyed { get; set; }

        public bool CopyTo { get; set; }
        public bool IsCompleted { get; set; }

        public bool AllowCopyFrom { get; set; }

        [StringLength(100)]
        public string CopiedFrom { get; set; }

        [StringLength(200)]
        public string AddressLine1 { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }

        public bool TypeUpdated { get; set; }

        public bool IsAlreadyCheckedPropertyType { get; set; }

        public bool IsLoadToPhone { get; set; }

    }




    public class Question : Entity
    {
        public int Question_Order { get; set; }

        [StringLength(100)]
        public string Question_Ref { get; set; }

        [StringLength(100)]
        public string Main_Element { get; set; }

        [StringLength(100)]
        public string Question_Heading { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string Question_Type { get; set; }

        [StringLength(100)]
        public string Apply_2nd_Question { get; set; }

        [StringLength(100)]
        public string Unit { get; set; }

        [StringLength(100)]
        public string SurveyType { get; set; }

        [StringLength(100)]
        public string DecenWtHomes { get; set; }

        public bool ExcludeFromClone { get; set; }

        [StringLength(100)]
        public string QuestionGroup { get; set; }

        [StringLength(100)]
        public string Display { get; set; }

        [StringLength(100)]
        public string LeaseholderQuestion { get; set; }

        [StringLength(100)]
        public string AnswerRangeFrom { get; set; }

        [StringLength(100)]
        public string AnswerRangeTo { get; set; }

        [StringLength(100)]
        public string AnswerRangeTextF { get; set; }

        [StringLength(100)]
        public string AnswerRangeTextT { get; set; }

        [StringLength(100)]
        public string LookAtRange { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }

        public bool finishSection { get; set; }

        public bool createPDF { get; set; }

        public bool isDate { get; set; }

        public bool NeedToHaveMedia { get; set; }

    }


    public class Option : Entity
    {
        [StringLength(100)]
        public string Question_Ref { get; set; }

        [StringLength(100)]
        public string Level { get; set; }

        public int Option_Number { get; set; }

        public int OptionID { get; set; }

        [StringLength(200)]
        public string Text { get; set; }

        [StringLength(100)]
        public string Jumpto { get; set; }

        public bool Choose { get; set; }
        public bool ConfirmationRequired { get; set; }

        [StringLength(100)]
        public string LifeGuide { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }

        public bool Disable2nd { get; set; }

        public bool createPDF { get; set; }

        public bool NeedToHaveMedia { get; set; }

    }


    public class RichMedia : Entity
    {
        [StringLength(100)]
        public string UPRN { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }

        [StringLength(200)]
        public string FileName { get; set; }

        [StringLength(400)]
        public string CloudPath { get; set; }

        [StringLength(100)]
        public string Question_Ref { get; set; }

        [ForeignKey("Option")]
        public Guid? Option_ID { get; set; }

        [IgnoreDataMember]
        [ForeignKey("Option_ID")]
        public Option Option { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }

        [StringLength(400)]
        public string PDFFileName { get; set; }

    }

    public class Survelem : Entity
    {
        [ForeignKey("Option")]
        public Guid? OptionID { get; set; }

        [IgnoreDataMember]
        [ForeignKey("OptionID")]
        public Option Option { get; set; }

        [ForeignKey("Option2ndry")]
        public Guid? OptionID2ndry { get; set; }

        [IgnoreDataMember]
        [ForeignKey("OptionID2ndry")]
        public Option Option2ndry { get; set; }

        [StringLength(100)]
        public string UPRN { get; set; }

        [StringLength(100)]
        public string Question_Ref { get; set; }

        [StringLength(200)]
        public string COMMENT { get; set; }

        [StringLength(200)]
        public string Freetext { get; set; }

        [StringLength(100)]
        public string BuildingType { get; set; }

        [StringLength(100)]
        public string DateOfSurvey { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }


                [StringLength(100)]
        public string SqT1 { get; set; }

                [StringLength(100)]
        public string SqN1 { get; set; }

                [StringLength(100)]
        public string SqT2 { get; set; }

                [StringLength(100)]
        public string SqN2 { get; set; }

                [StringLength(100)]
        public string SqT3 { get; set; }

                [StringLength(100)]
        public string SqN3 { get; set; }

                [StringLength(100)]
        public string SqT4 { get; set; }

                [StringLength(100)]
        public string SqN4 { get; set; }
                [StringLength(100)]
        public string SqT5 { get; set; }
                [StringLength(100)]
        public string SqN5 { get; set; }
                [StringLength(100)]
        public string SqT6 { get; set; }
                [StringLength(100)]
        public string SqN6 { get; set; }
                [StringLength(100)]
        public string SqT7 { get; set; }
                [StringLength(100)]
        public string SqN7 { get; set; }
                [StringLength(100)]
        public string SqT8 { get; set; }
                [StringLength(100)]
        public string SqN8 { get; set; }

                [StringLength(100)]
        public string SqT9 { get; set; }
                [StringLength(100)]
        public string SqN9 { get; set; }
                [StringLength(100)]
        public string SqT10 { get; set; }
                [StringLength(100)]
        public string SqN10 { get; set; }
                [StringLength(100)]
        public string SqT11 { get; set; }
                [StringLength(100)]
        public string SqT12 { get; set; }
                [StringLength(100)]
        public string SqT13 { get; set; }
                [StringLength(100)]
        public string SqT14 { get; set; }
                [StringLength(100)]
        public string SqT15 { get; set; }
                [StringLength(100)]
        public string SqN11 { get; set; }
                [StringLength(100)]
        public string SqN12 { get; set; }
                [StringLength(100)]
        public string SqN13 { get; set; }
                [StringLength(100)]
        public string SqN14 { get; set; }
                [StringLength(100)]
        public string SqN15 { get; set; }

        //[NotMapped]
        //public List<SurvelemSecond> SurvelemSeconds { get; set; }



    }


    //public class SurvelemSecond : Entity
    //{
    //    [ForeignKey("Survelem")]
    //    public Guid SurvelemId { get; set; }

    //    [IgnoreDataMember]
    //    [ForeignKey("SurvelemId")]
    //    public Survelem Survelem { get; set; }

    //    public bool TextOrNumber { get; set; }

    //    [StringLength(100)]
    //    public string Value { get; set; }

    //    [StringLength(100)]
    //    public string Name { get; set; }


    //}


    public class SurveyType : Entity
    {

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Value { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }
    }


    public class SurvelemMap : Entity
    {
        [StringLength(100)]
        public string Question_Ref { get; set; }

        [StringLength(100)]
        public string SqName { get; set; }

        [StringLength(100)]
        public string Question_Heading { get; set; }

        [StringLength(100)]
        public string Formats { get; set; }

        [StringLength(100)]
        public string SurveyType { get; set; }

        public long CustomerID { get; set; }

        [StringLength(100)]
        public string CustomerSurveyID { get; set; }


        public int Order { get; set; }
    }


    public class LogItem : Entity
    {
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public string ExceptionType { get; set; }
        public string Message { get; set; }
    }

    public class QAAddress : Entity
    {
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }


        //[ForeignKey("Surveyor")]
        public Guid SurveyorId { get; set; }

        //[IgnoreDataMember]
        //[ForeignKey("SurveyorId")]
        //public Surveyor Surveyor { get; set; }


        public bool IsCompleted { get; set; }



    }

    public class QAAddressComment : Entity
    {
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

             [StringLength(100)]
        public string QuestionRef { get; set; }

             [StringLength(400)]
        public string Text { get; set; }
    }

    public class AddressQuestionGroupStatus : Entity
    {
        [ForeignKey("Address")]
        public Guid AddressId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

             [StringLength(100)]
        public string Group { get; set; }

        public bool IsCompleted { get; set; }
    }
    



}