cd .\OddJob
nuget pack -IncludeReferencedProjects
cd ..
cd .\GlutenFree.OddJob.Serializable
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Execution.Akka
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.FileSystem
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.Sql.Common
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.SQLite
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.SqlServer
nuget pack -IncludeReferencedProjects
cd ..
