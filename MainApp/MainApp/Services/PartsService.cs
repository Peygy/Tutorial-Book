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
                            return new PartModel { Id = partId, Table = table, 
                            Children = await ChildrenFilter(await topicsData.Sections.ToListAsync(), table, title, filtre) };
                        }
                        var section = await topicsData.Sections.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (section != null) { section.Children = await ChildrenFilter(section.Children, table, title, filtre); } return section;

                    case "subsections":
                        if (partId == 0)
                        {
                            return new Section { Id = partId, Table = table, 
                            Children = await ChildrenFilter(await topicsData.Subsections.Include(p => p.Parent).ToListAsync(), table, title, filtre) };
                        }
                        var subsection = await topicsData.Subsections.Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (subsection != null) { subsection.Children = await ChildrenFilter(subsection.Children, table, title, filtre); } return subsection;

                    case "chapters":
                        if (partId == 0)
                        {
                            return new Subsection { Id = partId, Table = table, 
                            Children = await ChildrenFilter(await topicsData.Chapters.Include(p => p.Parent).ToListAsync(), table, title, filtre) };
                        }
                        var chapter = await topicsData.Chapters.Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (chapter != null) { chapter.Children = await ChildrenFilter(chapter.Children, table, title, filtre); } return chapter;

                    case "subchapters":
                        if (partId == 0)
                        {
                            return new Chapter { Id = partId, Table = table, 
                            Children =  await ChildrenFilter(await topicsData.Subchapters.Include(p => p.Parent).ToListAsync(), table, title, filtre) };
                        }
                        var subchapter = await topicsData.Subchapters.Include(p => p.Parent).Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (subchapter != null) { subchapter.Children = await ChildrenFilter(subchapter.Children, table, title, filtre); } return subchapter;

                    case "posts":
                        if (partId == 0)
                        {
                            return new Subchapter { Id = partId, Table = table, 
                            Children = await ChildrenFilter(await topicsData.Posts.Include(p => p.Parent).ToListAsync(), table, title, filtre) };
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


        public async Task<IEnumerable<T>?> ChildrenFilter<T>
        (IEnumerable<T>? children, string table, string? title, string? filtre) where T : PartModel
        {
            if (children == null) { return null; }
            if(title != null && (filtre == "child" || filtre == null))
            {
                children = children.Where(s => s.Title == title).ToList();
            }
            else if(title != null && filtre == "parent")
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
                return table switch
                {
                    "subsections" => (await topicsData.Sections.ToListAsync()).Cast<PartModel>().ToList(),
                    "chapters" => (await topicsData.Subsections.ToListAsync()).Cast<PartModel>().ToList(),
                    "subchapters" => (await topicsData.Chapters.ToListAsync()).Cast<PartModel>().ToList(),
                    "posts" => (await topicsData.Subchapters.ToListAsync()).Cast<PartModel>().ToList(),
                    _ => new List<PartModel>()
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return new List<PartModel>();
        }

        public async Task<string> GetPostContentAsync(string? contentId)
        {
            try
            {
                var content = await contentData.GetContentAsync(contentId);
                if (content != null) { return content; }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return "Пост ничего не содержит!";
        }



        public async Task<bool> AddPartAsync(PartModel newPart, string content)
        {
            try
            {
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
                        var section = await topicsData.Sections.FirstOrDefaultAsync(s => s.Id == part.Id);
                        if (section != null)
                        {
                            section.Title = part.Title;
                            section.CreatedDate = DateTime.UtcNow;

                            topicsData.Sections.Update(section);
                            await topicsData.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "subsections":
                        var subsection = await topicsData.Subsections.FirstOrDefaultAsync(s => s.Id == part.Id);
                        if (subsection != null)
                        {
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
                        var chapter = await topicsData.Chapters.FirstOrDefaultAsync(s => s.Id == part.Id);
                        if (chapter != null)
                        {
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
                        var subchapter = await topicsData.Subchapters.FirstOrDefaultAsync(s => s.Id == part.Id);
                        if (subchapter != null)
                        {
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
                        var post = await topicsData.Posts.FirstOrDefaultAsync(s => s.Id == part.Id);
                        if (post != null)
                        {
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
                return parentTable switch {
                    "sections" => (await topicsData.Sections.ToListAsync()).Any(s => s.Id!=partId && s.Title==title),

                    "subsections" => (await topicsData.Subsections.ToListAsync()).Any(s => s.Id!=partId && s.Title==title),

                    "chapters" => (await topicsData.Chapters.ToListAsync()).Any(s => s.Id!=partId && s.Title==title),

                    "subchapters" => (await topicsData.Subchapters.ToListAsync()).Any(s => s.Id!=partId && s.Title==title),

                    "posts" => (await topicsData.Posts.ToListAsync()).Any(s => s.Id!=partId && s.Title==title),
                    _ => true
                };
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
                        var section = await topicsData.Sections.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (section != null)
                        {
                            topicsData.Sections.Remove(section);
                            await topicsData.SaveChangesAsync();
                            return section.Id;
                        }
                        break;

                    case "subsections":
                        var subsection = await topicsData.Subsections.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (subsection != null)
                        {
                            topicsData.Subsections.Remove(subsection);
                            await topicsData.SaveChangesAsync();
                            return subsection.Id;
                        }
                        break;

                    case "chapters":
                        var chapter = await topicsData.Chapters.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (chapter != null)
                        {
                            topicsData.Chapters.Remove(chapter);
                            await topicsData.SaveChangesAsync();
                            return chapter.Id;
                        }
                        break;

                    case "subchapters":
                        var subchapter = await topicsData.Subchapters.Include(p => p.Children).FirstOrDefaultAsync(s => s.Id == partId);
                        if (subchapter != null)
                        {
                            topicsData.Subchapters.Remove(subchapter);
                            await topicsData.SaveChangesAsync();
                            return subchapter.Id;
                        }
                        break;

                    case "posts":
                        var post = await topicsData.Posts.FirstOrDefaultAsync(s => s.Id == partId);
                        if (post != null)
                        {
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
