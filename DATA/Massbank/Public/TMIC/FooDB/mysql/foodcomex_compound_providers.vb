REM  Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic.CodeGenerator
REM  MYSQL Schema Mapper
REM      for Microsoft VisualBasic.NET 2.1.0.2569

REM  Dump @2018/3/8 14:41:30


Imports System.Data.Linq.Mapping
Imports System.Xml.Serialization
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports MySqlScript = Oracle.LinuxCompatibility.MySQL.Scripting.Extensions

Namespace TMIC.FooDB.mysql

''' <summary>
''' ```SQL
''' 
''' --
''' 
''' DROP TABLE IF EXISTS `foodcomex_compound_providers`;
''' /*!40101 SET @saved_cs_client     = @@character_set_client */;
''' /*!40101 SET character_set_client = utf8 */;
''' CREATE TABLE `foodcomex_compound_providers` (
'''   `id` int(11) NOT NULL AUTO_INCREMENT,
'''   `foodcomex_compound_id` int(11) NOT NULL,
'''   `provider_id` int(11) NOT NULL,
'''   `created_at` datetime DEFAULT NULL,
'''   `updated_at` datetime DEFAULT NULL,
'''   PRIMARY KEY (`id`),
'''   KEY `index_foodcomex_compound_providers_on_foodcomex_compound_id` (`foodcomex_compound_id`),
'''   KEY `index_foodcomex_compound_providers_on_provider_id` (`provider_id`)
''' ) ENGINE=InnoDB AUTO_INCREMENT=1090 DEFAULT CHARSET=utf8;
''' /*!40101 SET character_set_client = @saved_cs_client */;
''' 
''' --
''' ```
''' </summary>
''' <remarks></remarks>
<Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes.TableName("foodcomex_compound_providers", Database:="foodb", SchemaSQL:="
CREATE TABLE `foodcomex_compound_providers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `foodcomex_compound_id` int(11) NOT NULL,
  `provider_id` int(11) NOT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `index_foodcomex_compound_providers_on_foodcomex_compound_id` (`foodcomex_compound_id`),
  KEY `index_foodcomex_compound_providers_on_provider_id` (`provider_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1090 DEFAULT CHARSET=utf8;")>
Public Class foodcomex_compound_providers: Inherits Oracle.LinuxCompatibility.MySQL.MySQLTable
#Region "Public Property Mapping To Database Fields"
    <DatabaseField("id"), PrimaryKey, AutoIncrement, NotNull, DataType(MySqlDbType.Int64, "11"), Column(Name:="id"), XmlAttribute> Public Property id As Long
    <DatabaseField("foodcomex_compound_id"), NotNull, DataType(MySqlDbType.Int64, "11"), Column(Name:="foodcomex_compound_id")> Public Property foodcomex_compound_id As Long
    <DatabaseField("provider_id"), NotNull, DataType(MySqlDbType.Int64, "11"), Column(Name:="provider_id")> Public Property provider_id As Long
    <DatabaseField("created_at"), DataType(MySqlDbType.DateTime), Column(Name:="created_at")> Public Property created_at As Date
    <DatabaseField("updated_at"), DataType(MySqlDbType.DateTime), Column(Name:="updated_at")> Public Property updated_at As Date
#End Region
#Region "Public SQL Interface"
#Region "Interface SQL"
    Private Shared ReadOnly INSERT_SQL As String = <SQL>INSERT INTO `foodcomex_compound_providers` (`id`, `foodcomex_compound_id`, `provider_id`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');</SQL>
    Private Shared ReadOnly REPLACE_SQL As String = <SQL>REPLACE INTO `foodcomex_compound_providers` (`id`, `foodcomex_compound_id`, `provider_id`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');</SQL>
    Private Shared ReadOnly DELETE_SQL As String = <SQL>DELETE FROM `foodcomex_compound_providers` WHERE `id` = '{0}';</SQL>
    Private Shared ReadOnly UPDATE_SQL As String = <SQL>UPDATE `foodcomex_compound_providers` SET `id`='{0}', `foodcomex_compound_id`='{1}', `provider_id`='{2}', `created_at`='{3}', `updated_at`='{4}' WHERE `id` = '{5}';</SQL>
#End Region
''' <summary>
''' ```SQL
''' DELETE FROM `foodcomex_compound_providers` WHERE `id` = '{0}';
''' ```
''' </summary>
    Public Overrides Function GetDeleteSQL() As String
        Return String.Format(DELETE_SQL, id)
    End Function
''' <summary>
''' ```SQL
''' INSERT INTO `foodcomex_compound_providers` (`id`, `foodcomex_compound_id`, `provider_id`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
''' ```
''' </summary>
    Public Overrides Function GetInsertSQL() As String
        Return String.Format(INSERT_SQL, id, foodcomex_compound_id, provider_id, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
    End Function

''' <summary>
''' <see cref="GetInsertSQL"/>
''' </summary>
    Public Overrides Function GetDumpInsertValue() As String
        Return $"('{id}', '{foodcomex_compound_id}', '{provider_id}', '{created_at}', '{updated_at}')"
    End Function


''' <summary>
''' ```SQL
''' REPLACE INTO `foodcomex_compound_providers` (`id`, `foodcomex_compound_id`, `provider_id`, `created_at`, `updated_at`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
''' ```
''' </summary>
    Public Overrides Function GetReplaceSQL() As String
        Return String.Format(REPLACE_SQL, id, foodcomex_compound_id, provider_id, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at))
    End Function
''' <summary>
''' ```SQL
''' UPDATE `foodcomex_compound_providers` SET `id`='{0}', `foodcomex_compound_id`='{1}', `provider_id`='{2}', `created_at`='{3}', `updated_at`='{4}' WHERE `id` = '{5}';
''' ```
''' </summary>
    Public Overrides Function GetUpdateSQL() As String
        Return String.Format(UPDATE_SQL, id, foodcomex_compound_id, provider_id, MySqlScript.ToMySqlDateTimeString(created_at), MySqlScript.ToMySqlDateTimeString(updated_at), id)
    End Function
#End Region
Public Function Clone() As foodcomex_compound_providers
                  Return DirectCast(MyClass.MemberwiseClone, foodcomex_compound_providers)
              End Function
End Class


End Namespace
