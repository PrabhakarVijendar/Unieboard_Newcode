// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using StructureMap;
using UniEBoard.Model.Adapters.Caching;
using UniEBoard.Model.Adapters.Mapping;
using UniEBoard.Model.Adapters.ExceptionHandling;
using UniEBoard.Model.Adapters.Logging;
using UniEBoard.Model.Adapters.Files;
using UniEBoard.Model.Interfaces.Adapter;
using UniEBoard.Model.Interfaces.DomainService;
using UniEBoard.Model.Interfaces.Repository;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.DomainServices;
using UniEBoard.Repository.Repositories;
using UniEBoard.Service.ApplicationServices;

namespace UniEBoard.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(x =>
            {
                x.Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
                // Resolve Adapters
                x.For<IObjectMapperAdapter>().Singleton().Use<AutoMapperAdapter>();
                x.For<ICacheAdapter>().Use<HttpContextCacheAdapter>();
                x.For<IExceptionManagerAdapter>().Use<EnterpriseLibraryExceptionManagerAdapter>();
                x.For<ILoggingServiceAdapter>().Use<EnterpriseLibraryLoggingManagerAdapter>();
                x.For<IFileManagerAdapter>().Use<FileManagerAdapter>();

                //Resolve Repositories
                x.For<IUserRepository>().Use<UserRepository>();
                x.For<IStudentRepository>().Use<StudentRepository>();
                x.For<IStaffRepository>().Use<StaffRepository>();
                x.For<IMembershipRepository>().Use<MembershipRepository>();
                x.For<IMessageRepository>().Use<MessageRepository>();
                x.For<ITaskRepository>().Use<TaskRepository>();
                x.For<IViewedMessageRepository>().Use<ViewedMessageRepository>();
                x.For<IAssignmentRepository>().Use<AssignmentRepository>();
                x.For<ICourseRepository>().Use<CourseRepository>();
                x.For<IFileRepository>().Use<FileRepository>();
                x.For<IBaseFileRepository>().Use<BaseFileRepository>();
                x.For<IUnitRepository>().Use<UnitRepository>();
                x.For<IModuleRepository>().Use<ModuleRepository>();
                x.For<ISubmissionRepository>().Use<SubmissionRepository>();
                x.For<IVideoRepository>().Use<VideoRepository>();
                x.For<IQuizRepository>().Use<QuizRepository>();
                x.For<IQuizEntryRepository>().Use<QuizEntryRepository>();
                x.For<IQuestionRepository>().Use<QuestionRepository>();
                x.For<IQuestionChoiceRepository>().Use<QuestionChoicesRepository>();
                x.For<IAnswerRepository>().Use<AnswerRepository>();
                x.For<IAnswerQuestionChoiceRepository>().Use<AnswerQuestionChoiceRepository>();
                x.For<IScheduleRepository>().Use<ScheduleRepository>();
                x.For<ITopicPostRepository>().Use<TopicPostRepository>();
                x.For<ITopicRepository>().Use<TopicRepository>();
                x.For<IDiscussionRepository>().Use<DiscussionRepository>();
                x.For<IBaseQuestionTopicRepository>().Use<BaseQuestionTopicRepository>();
                x.For<IAssetRepository>().Use<AssetRepository>();
                x.For<IModuleQuizRepository>().Use<ModuleQuizRepository>();
                x.For<IDepartmentRepository>().Use<DepartmentRepository>();
                x.For<ICourseRegistrationRepository>().Use<CourseRegistrationRepository>();
                x.For<ICourseModuleRepository>().Use<CourseModuleRepository>();
                x.For<IStaffCourseRepository>().Use<StaffCourseRepository>();

                //Resolve Domain Services
                x.For<IUserDomainService>().Use<UserDomainService>();
                x.For<IStudentDomainService>().Use<StudentDomainService>();
                x.For<IStaffDomainService>().Use<StaffDomainService>();
                x.For<IMembershipDomainService>().Use<MembershipDomainService>();
                x.For<ITypeDomainService>().Use<TypeDomainService>();
                x.For<IMessageDomainService>().Use<MessageDomainService>();
                x.For<ITaskDomainService>().Use<TaskDomainService>();
                x.For<IStudentViewedMessageDomainService>().Use<StudentViewedMessageDomainService>();
                x.For<IAssignmentDomainService>().Use<AssignmentDomainService>();
                x.For<ICourseDomainService>().Use<CourseDomainService>();
                x.For<IFileDomainService>().Use<FileDomainService>();
                x.For<IBaseFileDomainService>().Use<BaseFileDomainService>();
                x.For<IUnitDomainService>().Use<UnitDomainService>();
                x.For<ISubmissionDomainService>().Use<SubmissionDomainService>();
                x.For<IVideoDomainService>().Use<VideoDomainService>();
                x.For<IModuleDomainService>().Use<ModuleDomainService>();
                x.For<IQuizDomainService>().Use<QuizDomainService>();
                x.For<IQuizEntryDomainService>().Use<QuizEntryDomainService>();
                x.For<IQuestionDomainService>().Use<QuestionDomainService>();
                x.For<IQuestionChoiceDomainService>().Use<QuestionChoiceDomainService>();
                x.For<IAnswerDomainService>().Use<AnswerDomainService>();
                x.For<IAnswerQuestionChoiceDomainService>().Use<AnswerQuestionChoiceDomainService>();
                x.For<IScheduleDomainService>().Use<ScheduleDomainService>();
                x.For<ITopicPostDomainService>().Use<TopicPostDomainService>();
                x.For<ITopicDomainService>().Use<TopicDomainService>();
                x.For<IDiscussionDomainService>().Use<DiscussionDomainService>();
                x.For<IBaseQuestionTopicDomainService>().Use<BaseQuestionTopicDomainService>();
                x.For<IAssetDomainService>().Use<AssetDomainService>();
                x.For<IModuleQuizDomainService>().Use<ModuleQuizDomainService>();
                x.For<IDepartmentDomainService>().Use<DepartmentDomainService>();
                x.For<ICourseRegistrationDomainService>().Use<CourseRegistrationDomainService>();

                //Resolve Application Services
                x.For<IAssignmentTaskSubmissionAppService>().Use<AssignmentTaskSubmissionAppService>();
                x.For<ICourseModuleAppService>().Use<CourseModuleAppService>();
                x.For<IStudentAppService>().Use<StudentAppService>();
                x.For<ITypeAppService>().Use<TypeAppService>();
                x.For<IMessageAppService>().Use<MessageAppService>();
                x.For<IStaffAppService>().Use<StaffAppService>();
                x.For<IUnitModuleAppService>().Use<UnitModuleAppService>();
                x.For<IFileAppService>().Use<FileAppService>();
                x.For<IQuizAppService>().Use<QuizAppService>();
                x.For<IQuestionAppService>().Use<QuestionAppService>();
                x.For<IAnswerAppService>().Use<AnswerAppService>();
                x.For<IScheduleAppService>().Use<ScheduleAppService>();
                x.For<IDiscussionAppService>().Use<DiscussionAppService>();
                x.For<IBaseQuestionTopicAppService>().Use<BaseQuestionTopicAppService>();
                x.For<ISecurityAppService>().Use<SecurityAppService>();
                x.For<IAssetAppService>().Use<AssetAppService>();
                x.For<IVideoAppService>().Use<VideoAppService>();
                x.For<IUserAppService>().Use<UserAppService>();

            });
            return ObjectFactory.Container;
        }
    }
}
