using System;
using System.Collections.Generic;
using System.Linq;
using HuntersService.Entities;
using Netmera;
using Newtonsoft.Json;
using Omu.ValueInjecter;

namespace HuntersService.Contracts.Base
{
	public abstract class RequestHandler<TRequest, TReply> : IRequestHandler
		where TRequest : BaseRequest
		where TReply : BaseReply,new()
	{
		protected RequestContext RequestContext { get; private set; }

	    protected MyDbContext DbContext
	    {
	        get { return RequestContext.DbContext; }
	    }

		public BaseReply Execute(BaseRequest request, RequestContext requestContext)
		{
            RequestContext = requestContext;
		    try
		    {
                return Execute((TRequest)request);
		    }
		    catch (Exception exc)
		    {
		        using (var dbContext = new MyDbContext())
		        {

		            var item = new LogItem()
		            {
		                Date = DateTime.UtcNow,
		                Message = request.GetType() + " " + exc.Message,
		                Type = (int) ELogItemType.Exception,
		                ExceptionType = exc.ToString()
		            };

		            if (exc.InnerException != null)
		            {
		                item.Message += " Inner: " + exc.InnerException.Message;
		            }

		            dbContext.LogItems.Add(item);
		            dbContext.SaveChanges();
		        }

		        return new TReply(){IsSuccess = false,Data = exc.Message};
		    }

		}

		protected abstract TReply Execute(TRequest request);


	    protected T CheckUser<T>(BaseAuthRequest request) where T:BaseReply,new()
	    {
	        var user = DbContext.Surveyors.Find(request.UserAuthId);

	        if (user != null) return null;

            return new T(){IsSuccess = false,NotFoundUser = true};

	    }


        protected BaseReply SaveEntities<T>(List<Entity> items, List<string> addIgnoreProps = null) where T:Entity,new()
        {
            var reply = new BaseReply() { };

            foreach (var i in items.Where(x=>x.IsDeletedOnClient))
            {
                var db = DbContext.Set<T>().Find(i.Id);
                if (db != null)
                {
                    DbContext.Set<T>().Remove(db);
                }
            }

            foreach (var i in items.Where(x => !x.IsDeletedOnClient))
            {
                bool add = false;

                var db = DbContext.Set<T>().Find(i.Id);
                if (db == null)
                {
                    add = true;
                    db = new T(){Id=i.Id};
                }
                else
                {
                    db.UpdateDate = DateTime.UtcNow;
                }

                var ignore = new List<String> {"Id", "CreateDate", "NetmeraId", "LogItemId"};

                if (addIgnoreProps != null)
                {
                    ignore.AddRange(addIgnoreProps);
                }

                db.InjectFrom(new IngnoreProps(ignore.ToArray()), i);

                if (add)
                    DbContext.Set<T>().Add(db);
            }

            DbContext.SaveChanges();

            return reply;
        }



        protected void SearchObjectsWithPaging<T, V>(NetmeraService service, Func<T, V, bool> action = null, Action<List<V>> saveAction = null )
            where T : Entity, new()
            where V : class
        {
            var max = -1;
            var page = 1000;

            if (typeof(V) == typeof(AddressOld))
            {
                service.setSortBy("UPRN");

            }
            else if (typeof(V) == typeof(SurvelemOld))
            {
                service.setSortBy("id");
            }
            else if (typeof(V) == typeof(QuestionOld))
            {
                service.setSortBy("Question_Order");
            }
            else
            {
                service.setSortBy("Path");
            }

            service.setSortOrder(NetmeraService.SortOrder.ascending);

            var c = service.count();
            var loadedCount = 0;
            var pages = c / page;

            for (int i = 0; i <= pages; i++)
            {
                service.setPage(i);
                service.setMax(page);
                var objects = new List<V>();
                var list = service.search();
                foreach (var netmeraContent in list)
                {
                    var o = JsonConvert.DeserializeObject<V>(netmeraContent.data.ToString());

                    if (typeof (V) == typeof (AddressOld))
                    {
                        (o as AddressOld).UpdateDate = netmeraContent.getUpdateDate();
                    }

                    objects.Add(o);
                }

                if (saveAction == null)
                {
                    Dictionary<Guid, V> oldData = new Dictionary<Guid, V>();

                    var entities = objects.Select(x =>
                    {
                        var s = new T();

                        var r = action(s, x as V);

                        if (!r)
                        {
                            s.InjectFrom(x);

                        }

                        if (!oldData.ContainsKey(s.Id))
                        {
                            oldData.Add(s.Id, x);
                        }

                        return s;
                    }).ToList();

                    foreach (var entity in entities)
                    {
                        if (true)
                        {
                            if (DbContext.Set<T>().Any(x => x.NetmeraId == entity.NetmeraId))
                            {
                                continue;
                            }
                        }

                        DbContext.Set<T>().Add(entity);
                    }
                    DbContext.SaveChanges();


                    loadedCount += objects.Count;
                }
                else
                {

                    saveAction(objects);

                    loadedCount += objects.Count;

                }


             




             


                if (max != -1)
                {
                    if (loadedCount > max)
                    {
                        break;
                    }
                }

            }

        }


	}

    public class IngnoreProps : ConventionInjection
    {
        private readonly string[] _ignores;

        public IngnoreProps(params string[] ignores)
        {
            _ignores = ignores;
        }

        protected override bool Match(ConventionInfo c)
        {
            return (_ignores == null || !_ignores.Contains(c.SourceProp.Name)) &&
                   c.SourceProp.Name == c.TargetProp.Name
                   && c.SourceProp.Type == c.TargetProp.Type;
        }
    }
}
