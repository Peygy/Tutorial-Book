using Microsoft.EntityFrameworkCore;
using MainApp.Models;

namespace MainApp.Services
{
    public class PartsService : IPartsService
    {
        // Data context for parts
        private readonly TopicsContext topicsData;
        private readonly IMongoService contentData;
        // Logger for exceptions
        private readonly ILogger<PartsService> log;

        public PartsService(TopicsContext topicsData, MongoService contentData, ILogger<PartsService> log)
        {
            this.topicsData = topicsData;
            this.contentData = contentData;
            this.log = log;
        }



        public async Task<PartModel?> GetPartAsync(int partId, string table, string? title, string? filtre)
        {
            try
            {
                switch (table)
                {
                    case "sections":
                        if (partId == 0)
                        {
                            var sections = await topicsData.Sections.ToListAsync();
                            return new PartModel { Id = partId, Table = table, 
                            Children = await ChildrenFilter<Section>(sections.Cast<Section>(), table, title, filtre) };
                        }
                        var section = await topicsData.Sections
                             .Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        section.Children = await ChildrenFilter<Subsection>(section.Children, table, title, filtre);
                        return section;

                    case "subsections":
                        if (partId == 0)
                        {
                            var subsections = await topicsData.Subsections.Include(p => p.Parent).ToListAsync();
                            return new Section { Id = partId, Table = table, 
                            Children = await ChildrenFilter<Subsection>(subsections.Cast<Subsection>(), table, title, filtre) };
                        }
                        var subsection = await topicsData.Subsections
                            .Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        subsection.Children = await ChildrenFilter<Chapter>(subsection.Children, table, title, filtre);
                        return subsection;

                    case "chapters":
                        if (partId == 0)
                        {
                            var chapters = await topicsData.Chapters.Include(p => p.Parent).ToListAsync();
                            return new Subsection { Id = partId, Table = table, 
                            Children = await ChildrenFilter<Chapter>(chapters.Cast<Chapter>(), table, title, filtre) };
                        }
                        var chapter = await topicsData.Chapters
                            .Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        chapter.Children = await ChildrenFilter<Subchapter>(chapter.Children, table, title, filtre);
                        return chapter;

                    case "subchapters":
                        if (partId == 0)
                        {
                            var subchapters = await topicsData.Subchapters.Include(p => p.Parent).ToListAsync();
                            return new Chapter { Id = partId, Table = table, 
                            Children =  await ChildrenFilter<Subchapter>(subchapters.Cast<Subchapter>(), table, title, filtre) };
                        }
                        var subchapter = await topicsData.Subchapters
                            .Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        subchapter.Children = await ChildrenFilter<Post>(subchapter.Children, table, title, filtre);
                        return subchapter;

                    case "posts":
                        if (partId == 0)
                        {
                            var posts = await topicsData.Posts.Include(p => p.Parent).ToListAsync();
                            return new Subchapter { Id = partId, Table = table, 
                            Children = await ChildrenFilter<Post>(posts.Cast<Post>(), table, title, filtre) };
                        }
                        return await topicsData.Posts.FirstOrDefaultAsync(s => s.Id == partId);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<T>> ChildrenFilter<T>
        (IEnumerable<T> children, string table, string? title, string? filtre) where T : PartModel
        {      
            if(title != null && (filtre == "child" || filtre == null))
            {
                children = children.Where(s => s.Title == title).ToList();
            }
            if(title != null && filtre == "parent")
            {
                var parent = (await GetAllParentsAsync(table)).FirstOrDefault(s => s.Title == title) ?? new PartModel{ Id = 0 };
                children = children.Where(s => s.ParentId == parent.Id).ToList();
            }

            return children;
        }

        public async Task<IEnumerable<PartModel>> GetAllParentsAsync(string table)
        {
            try
            {
                switch (table)
                {
                    case "subsections":
                        var allSections = await topicsData.Sections.ToListAsync();
                        return allSections.Cast<PartModel>().ToList();

                    case "chapters":
                        var allSubsections = await topicsData.Subsections.ToListAsync();
                        return allSubsections.Cast<PartModel>().ToList();

                    case "subchapters":
                        var allChapters = await topicsData.Chapters.ToListAsync();
                        return allChapters.Cast<PartModel>().ToList();

                    case "posts":
                        var allSubchapters = await topicsData.Subchapters.ToListAsync();
                        return allSubchapters.Cast<PartModel>().ToList();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }

        public async Task<string> GetPostContentAsync(string? contentId)
        {
            try
            {
                return (await contentData.GetContentAsync(contentId)).Content;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }



        public async Task<bool> AddPartAsync(PartModel newPart, string content)
        {
            try
            {
                if(newPart.ParentId != 0)
                {
                    newPart.Table = newPart.Table switch{
                        "sections" => "subsections",
                        "subsections" => "chapters",
                        "chapters" => "subchapters",
                        "subchapters" => "posts",
                        _ => string.Empty,
                    };
                }

                switch (newPart.Table)
                {
                    case "sections":
                        if (!topicsData.Sections.Any(s => s.Title == newPart.Title))
                        {
                            await topicsData.Sections.AddAsync(new Section { 
                                Title = newPart.Title, Table = newPart.Table, ParentId = newPart.ParentId     
                            });
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "subsections":
                        if (!topicsData.Subsections.Any(s => s.Title == newPart.Title))
                        {
                            await topicsData.Subsections.AddAsync(new Subsection { 
                                Title = newPart.Title, Table = newPart.Table, ParentId = newPart.ParentId
                            });
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "chapters":
                        if (!topicsData.Chapters.Any(s => s.Title == newPart.Title))
                        {
                            await topicsData.Chapters.AddAsync(new Chapter { 
                                Title = newPart.Title, Table = newPart.Table, ParentId = newPart.ParentId
                            });
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "subchapters":
                        if (!topicsData.Subchapters.Any(s => s.Title == newPart.Title))
                        {
                            await topicsData.Subchapters.AddAsync(new Subchapter { 
                                Title = newPart.Title, Table = newPart.Table, ParentId = newPart.ParentId
                            });
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "posts":
                        if (!topicsData.Posts.Any(s => s.Title == newPart.Title))
                        {
                            await topicsData.Posts.AddAsync(new Post { 
                                Title = newPart.Title, Table = newPart.Table, ParentId = newPart.ParentId,
                                ContentId = await contentData.AddContentAsync(new ContentModel { Content = content} )
                            });
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }



        public async Task<bool> UpdatePartAsync(PartModel part, string newContent)
        {
            try 
            {
                switch (part.Table)
                {
                    case "sections":
                        if (!topicsData.Sections.Any(s => s.Id != part.Id && s.Title == part.Title))
                        {
                            var section = await topicsData.Sections.FirstOrDefaultAsync(s => s.Id == part.Id);
                            section.Title = part.Title;
                            section.CreatedDate = DateTime.UtcNow;

                            topicsData.Sections.Update(section);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "subsections":
                        if (!topicsData.Subsections.Any(s => s.Id != part.Id && s.Title == part.Title))
                        {
                            var subsection = await topicsData.Subsections.FirstOrDefaultAsync(s => s.Id == part.Id);
                            subsection.Title = part.Title;
                            subsection.CreatedDate = DateTime.UtcNow;

                            if (topicsData.Sections.Any(s => s.Id == part.ParentId))
                                subsection.ParentId = part.ParentId;

                            topicsData.Subsections.Update(subsection);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "chapters":
                        if (!topicsData.Chapters.Any(s => s.Id != part.Id && s.Title == part.Title))
                        {
                            var chapter = await topicsData.Chapters.FirstOrDefaultAsync(s => s.Id == part.Id);
                            chapter.Title = part.Title;
                            chapter.CreatedDate = DateTime.UtcNow;

                            if (topicsData.Subsections.Any(s => s.Id == part.ParentId))
                                chapter.ParentId = part.ParentId;

                            topicsData.Chapters.Update(chapter);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "subchapters":
                        if (!topicsData.Subchapters.Any(s => s.Id != part.Id && s.Title == part.Title))
                        {
                            var subchapter = await topicsData.Subchapters.FirstOrDefaultAsync(s => s.Id == part.Id);
                            subchapter.Title = part.Title;
                            subchapter.CreatedDate = DateTime.UtcNow;

                            if (topicsData.Chapters.Any(s => s.Id == part.ParentId))
                                subchapter.ParentId = part.ParentId;

                            topicsData.Subchapters.Update(subchapter);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "posts":
                        if (!topicsData.Posts.Any(s => s.Id != part.Id && s.Title == part.Title))
                        {
                            var post = await topicsData.Posts.FirstOrDefaultAsync(s => s.Id == part.Id);
                            await contentData.UpdateContentAsync(new ContentModel { Id = post.ContentId, Content = newContent });
                            post.Title = part.Title;
                            post.CreatedDate = DateTime.UtcNow;

                            if (topicsData.Subchapters.Any(s => s.Id == part.ParentId))
                                post.ParentId = part.ParentId;

                            topicsData.Posts.Update(post);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return false;
        }


        public async Task<bool> CheckTitleExistanceAsync(int partId, string parentTable, string title)
        {
            try
            {
                switch (parentTable)
                {
                    case "sections":
                        var sections = await topicsData.Sections.ToListAsync();                       
                        return sections.Any(s => s.Id!=partId && s.Title==title);

                    case "subsections":
                        var subsections = await topicsData.Subsections.ToListAsync();
                        return subsections.Any(s => s.Id!=partId && s.Title==title);

                    case "chapters":
                        var chapters = await topicsData.Chapters.ToListAsync();
                        return chapters.Any(s => s.Id!=partId && s.Title==title);

                    case "subchapters":
                        var subchapters = await topicsData.Subchapters.ToListAsync();
                        return subchapters.Any(s => s.Id!=partId && s.Title==title);

                    case "posts":
                        var posts = await topicsData.Posts.ToListAsync();
                        return posts.Any(s => s.Id!=partId && s.Title==title);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return true;
        }



        public async Task<int?> RemovePartAsync(int partId, string table)
        {
            try
            {
                switch (table)
                {
                    case "sections":
                        if (topicsData.Sections.Any(s => s.Id == partId))
                        {
                            var section = await topicsData.Sections.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                            foreach(var subsection in section.Children)
                            {
                                subsection.ParentId = 0;
                                await topicsData.SaveChangesAsync();
                            }

                            topicsData.Sections.Remove(section);
                            await topicsData.SaveChangesAsync();
                            return section.Id;
                        }
                        break;

                    case "subsections":
                        if (topicsData.Subsections.Any(s => s.Id == partId))
                        {
                            var subsection = await topicsData.Subsections.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                            foreach (var chapter in subsection.Children)
                            {
                                chapter.ParentId = 0;
                                await topicsData.SaveChangesAsync();
                            }

                            topicsData.Subsections.Remove(subsection);
                            await topicsData.SaveChangesAsync();
                            return subsection.Id;
                        }
                        break;

                    case "chapters":
                        if (topicsData.Chapters.Any(s => s.Id == partId))
                        {
                            var chapter = await topicsData.Chapters.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                            foreach (var subchapter in chapter.Children)
                            {
                                subchapter.ParentId = 0;
                                await topicsData.SaveChangesAsync();
                            }

                            topicsData.Chapters.Remove(chapter);
                            await topicsData.SaveChangesAsync();
                            return chapter.Id;
                        }
                        break;

                    case "subchapters":
                        if (topicsData.Subchapters.Any(s => s.Id == partId))
                        {
                            var subchapter = await topicsData.Subchapters.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                            foreach (var post in subchapter.Children)
                            {
                                post.ParentId = 0;
                                await topicsData.SaveChangesAsync();
                            }

                            topicsData.Subchapters.Remove(subchapter);
                            await topicsData.SaveChangesAsync();
                            return subchapter.Id;
                        }
                        break;

                    case "posts":
                        if (topicsData.Posts.Any(s => s.Id == partId))
                        {
                            var post = await topicsData.Posts.FirstOrDefaultAsync(s => s.Id == partId);
                            topicsData.Posts.Remove(post);
                            await topicsData.SaveChangesAsync();
                            await contentData.RemoveContentAsync(post.ContentId);
                            return post.Id;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }
    }
}
