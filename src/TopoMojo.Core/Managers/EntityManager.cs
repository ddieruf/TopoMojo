using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TopoMojo.Abstractions;
using TopoMojo.Core.Data;
using TopoMojo.Core.Entities;
using TopoMojo.Core.Entities.Extensions;

namespace TopoMojo.Core
{
    public class EntityManager<T>
        where T : Entity, new()
    {
        public EntityManager(
            TopoMojoDbContext db,
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver
        )
        {
            _db = db;
            _logger = mill?.CreateLogger(this.GetType());
            _options = options;
            _profileResolver = profileResolver;
            LoadProfileAsync().Wait();
        }

        protected readonly TopoMojoDbContext _db;
        protected Profile _user;
        protected readonly ILogger _logger;
        protected readonly IProfileResolver _profileResolver;
        protected readonly CoreOptions _options;
        protected readonly IOptions<CoreOptions> _optAccessor;

        public virtual async Task<SearchResult<T>> ListAsync(Search search)
        {
            IQueryable<T> q = ListQuery(search);
            SearchResult<T> result = new SearchResult<T>();
            result.Search = search;
            result.Total = q.Count();
            result.Results = await (
                q.OrderBy(o=>o.Name)
                .ApplyPaging(search).ToArrayAsync());
            return result;
        }

        protected virtual IQueryable<T> ListQuery(Search search)
        {
            IQueryable<T> q = _db.Set<T>();

            if (search.Term.HasValue())
            {
                q = q.Where(o => o.Name.IndexOf(search.Term, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            return q;
        }


        public virtual async Task<T> LoadAsync(int id)
        {
            return await _db.Set<T>().FindAsync(id);
        }

        public virtual async Task<T> SaveAsync (T t)
        {
            Normalize(t);
            if (t.Id > 0)
                _db.Update(t);
            else
                _db.Add(t);
            await _db.SaveChangesAsync();
            return t;
        }

        public virtual async Task<T> SaveSummaryAsync(T t)
        {
            T entity = null;
            Normalize(t);

            if (t.Id > 0)
            {
                entity = _db.Set<T>().Find(t.Id);
                entity.Name = t.Name;
                _db.Update(entity);
            }
            else
            {
                entity = new T();
                entity.Name = t.Name;
                _db.Add(t);
            }
            await _db.SaveChangesAsync();

            return t;
        }

        public virtual async Task<List<T>> SaveRangeAsync(List<T> list)
        {
            _db.UpdateRange(list.Where(x => x.Id > 0));
            _db.AddRange(list.Where(x => x.Id == 0));

            await _db.SaveChangesAsync();

            return list;
        }

        public virtual async Task DeleteAsync(int id)
        {
            T t = _db.Set<T>().Find(id);
            if (t != null)
            {
                _db.Remove(t);
                await _db.SaveChangesAsync();
            }
        }

        protected virtual void Normalize(T t)
        {
            if (!t.GlobalId.HasValue())
                t.GlobalId = Guid.NewGuid().ToString();

            if (t.WhenCreated == DateTime.MinValue)
                t.WhenCreated = DateTime.UtcNow;

            if (t.Name.HasValue() && t.Name.Length > 100)
                t.Name = t.Name.Substring(0,100);

        }

        protected async Task LoadProfileAsync()
        {
            _user = _profileResolver.Profile;
            if (_user.Id == 0)
            {
                Profile person = await _db.Profiles
                    .Where(p => p.GlobalId == _user.GlobalId)
                    .SingleOrDefaultAsync();

                if (person == null)
                {
                    _db.Profiles.Add(new Profile
                    {
                        GlobalId = _user.GlobalId,
                        Name = _user.Name ?? "Anonymous",
                        WhenCreated = System.DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
                _user = person;
            }
        }
    }
}
