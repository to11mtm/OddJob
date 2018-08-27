cd .\OddJob
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Execution.Akka
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.FileSystem
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.SQL.Common
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.SQLite
nuget pack -IncludeReferencedProjects
cd ..
cd .\OddJob.Storage.SqlServer
nuget pack -IncludeReferencedProjects
cd ..
