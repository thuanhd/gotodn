﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GotoDN.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GotoDN.Web.Models;
using Microsoft.EntityFrameworkCore;
using GotoDN.Entities;
using GotoDN.Web.Authentication;
using GotoDN.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace GotoDN.Web.Controllers
{
    [Route("place")]
    public class PlaceController : BaseController
    {
        const string GGKEY = "AIzaSyCywwRaPWRhT1xRYsOk-Dw4PfC2uvbsaKQ";
        const string GGURL = @"https://maps.googleapis.com/maps/api/";

        public PlaceController(HTRepository repository) : base(repository)
        {
        }

        [HttpGet, Route("get-all")]
        [AllowAnonymous]
        public List<PlaceModel> GetAll()
        {
            var entities = this.HTRepository.PlaceRepository.GetAll()
                .Include("PlaceLanguages.Image")
                .Include("PlaceLanguages.Icon")
                .Take(1000).ToList();

            var models = entities.Select(x => AutoMapper.Mapper.Map<Place, PlaceModel>(x)).ToList();

            return models;
        }

        [HttpPost, Route("create-place")]
        [HTAuthorize]
        public PlaceModel CreatePlace()
        {
            var entity = new Place();
            entity.CreatedDate = Common.DateTimeHelper.GetDateTimeNow();
            entity.UpdatedDate = Common.DateTimeHelper.GetDateTimeNow();

            entity.PlaceLanguages = new List<PlaceLanguage>()
            {
                new PlaceLanguage()
                {
                    Title = "Place's Name",
                    Language = LanguageEnums.English,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new PlaceLanguage()
                {
                    Title = "",
                    Language = LanguageEnums.Vietnamese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new PlaceLanguage()
                {
                    Title = "",
                    Language = LanguageEnums.France,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new PlaceLanguage()
                {
                    Title = "",
                    Language = LanguageEnums.Chinese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new PlaceLanguage()
                {
                    Title = "",
                    Language = LanguageEnums.Korean,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new PlaceLanguage()
                {
                    Title = "",
                    Language = LanguageEnums.Japanese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                }
            };

            this.HTRepository.PlaceRepository.Save(entity);
            this.HTRepository.Commit();
            return AutoMapper.Mapper.Map<Place, PlaceModel>(entity);
        }

        [HttpPost, Route("delete-place")]
        [HTAuthorize]
        public bool DeletePlace([FromBody]int Id)
        {
            var entity = this.HTRepository.PlaceRepository.GetAll()
                .FirstOrDefault(x => x.Id == Id);
            if (entity == null) return false;
            this.HTRepository.PlaceRepository.Delete(entity);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("delete-places")]
        [HTAuthorize]
        public bool DeletePlaces([FromBody]List<int> Ids)
        {
            var entities = this.HTRepository.PlaceRepository.GetAll()
                .Where(x => Ids.Contains(x.Id));
            if (entities == null) return false;
            this.HTRepository.PlaceRepository.Delete(entities);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("update-place")]
        [HTAuthorize]
        public bool UpdatePlace([FromBody]PlaceModel model)
        {
            if (model == null) return false;
            var entity = this.HTRepository.PlaceRepository.GetAll()
                .Include("PlaceLanguages.Image")
                .Include("PlaceLanguages.Icon")
                .Include("PlaceLanguages.PlaceImages.Image")
                .Include("PlaceLanguages.PlaceMoreInfo.Icon")
                .FirstOrDefault(x => x.Id == model.Id);
            if (entity == null)
            {
                entity = new Place();
            }

            var city = model.CityId.HasValue ? this.HTRepository.CityRepository.GetAll().FirstOrDefault(c => c.Id == model.CityId) : null;
            var district = model.DistrictId.HasValue ? this.HTRepository.DistrictRepository.GetAll().FirstOrDefault(c => c.Id == model.DistrictId) : null;
            var fullAddress = GetFullAddress(model.Address, district != null ? district.Name : "", city != null ? city.Name : "");
            var location = GetLocationFromAddress(fullAddress).Result;

            entity.UpdatedDate = DateTimeHelper.GetDateTimeNow();
            entity.Address = model.Address;
            entity.CityId = model.CityId;
            entity.CloseTime = model.CloseTime;
            entity.DistrictId = model.DistrictId;
            entity.EndDate = model.EndDate;
            entity.IsCategorySlider = model.IsCategorySlider;
            entity.IsHomeSlider = model.IsHomeSlider;
            entity.IsDistrictGovernment = model.IsDistrictGovernment;
            entity.Latitude = location != null ? location.Lat : (decimal?)null;
            entity.Longitude = location != null ? location.Lng : (decimal?)null;
            entity.OpenTime = model.OpenTime;
            entity.Phone = model.Phone;
            entity.Fax = model.Fax;
            entity.Rating = model.Rating;
            entity.StartDate = model.StartDate;
            entity.Website = model.Website;
            entity.HTServiceId = model.HTServiceId;
            entity.CategoryId = model.CategoryId;

            if (entity.PlaceLanguages == null || entity.PlaceLanguages.Count == 0)
            {
                entity.PlaceLanguages = model.PlaceLanguages.Select(p => new PlaceLanguage
                {
                    Title = p.Title,
                    ImageId = p.Image != null ? p.Image.Id : (int?)null,
                    IconId = p.Icon != null ? p.Icon.Id : (int?)null,
                    Description = p.Description,
                    PlaceImages = p.PlaceImages != null ? p.PlaceImages.Take(30).Select(t => new PlaceImage
                    {
                        ImageId = t.ImageId,
                        PlaceLangId = p.Id,
                    }).ToList() : null,
                    PlaceMoreInfo = p.PlaceMoreInfo != null ? p.PlaceMoreInfo.Take(10).Select((t, index) => new PlaceMoreInfo
                    {
                        IconId = t.Icon != null ? t.Icon.Id : (int?)null,
                        IsHalf = t.IsHalf,
                        Name = t.Name,
                        Value = t.Value,
                        Order = index
                    }).ToList() : null,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    Language = p.Language
                }).ToList();
            }
            else
            {
                foreach (var item in entity.PlaceLanguages)
                {
                    var en = model.PlaceLanguages.FirstOrDefault(x => x.Id == item.Id);
                    if (en != null)
                    {
                        item.Title = en.Title;
                        item.ImageId = en.Image != null ? en.Image.Id : (int?)null;
                        item.IconId = en.Icon != null ? en.Icon.Id : (int?)null;
                        item.Description = en.Description;
                        if (item.PlaceImages == null) item.PlaceImages = new List<PlaceImage>();
                        item.PlaceImages.Clear();
                        item.PlaceImages.AddRange(en.PlaceImages.Take(30).Select(t => new PlaceImage
                        {
                            ImageId = t.ImageId,
                            PlaceLangId = en.Id,
                        }));

                        item.PlaceMoreInfo = en.PlaceMoreInfo.Take(10).Select((t, index) => new PlaceMoreInfo
                        {
                            Id = 0,
                            IconId = t.Icon != null ? t.Icon.Id : (int?)null,
                            IsHalf = t.IsHalf,
                            Name = t.Name,
                            Value = t.Value,
                            Order = index
                        }).ToList();
                        item.UpdatedDate = DateTimeHelper.GetDateTimeNow();
                    }
                }
            }

            this.HTRepository.PlaceRepository.Save(entity);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("add-language")]
        [HTAuthorize]
        public PlaceLanguageModel AddLanguage([FromBody]PlaceLanguageModel model)
        {
            if (model == null) return null;
            var CatEntity = this.HTRepository.PlaceRepository.GetAll()
                .FirstOrDefault(x => x.Id == model.PlaceId.GetValueOrDefault());
            if (CatEntity == null) return null;
            if (CatEntity.PlaceLanguages == null) CatEntity.PlaceLanguages = new List<PlaceLanguage>();

            var LangEntity = new PlaceLanguage();
            LangEntity.PlaceId = model.PlaceId;
            LangEntity.Language = model.Language;
            LangEntity.Title = model.Title;
            LangEntity.UpdatedDate = DateTimeHelper.GetDateTimeNow();
            LangEntity.CreatedDate = DateTimeHelper.GetDateTimeNow();

            CatEntity.PlaceLanguages.Add(LangEntity);
            this.HTRepository.PlaceRepository.Save(CatEntity);
            this.HTRepository.Commit();

            return AutoMapper.Mapper.Map<PlaceLanguage, PlaceLanguageModel>(LangEntity);
        }

        [HttpPost, Route("add-all-language")]
        [HTAuthorize]
        public PlaceModel AddAllLanguage([FromBody]int Id)
        {
            var PlaceEntity = this.HTRepository.PlaceRepository.GetAll()
                .Include(x => x.PlaceLanguages)
                .FirstOrDefault(x => x.Id == Id);

            if (PlaceEntity == null) return null;

            foreach (var lang in LanguageArrays.GetLanguageArray())
            {
                if (!PlaceEntity.PlaceLanguages.Any(x => x.Language == (LanguageEnums)lang))
                {
                    PlaceEntity.PlaceLanguages.Add(new PlaceLanguage()
                    {
                        Language = (LanguageEnums)lang,
                        PlaceId = Id,
                        Title = "",
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    });
                }
            }
            this.HTRepository.PlaceRepository.Save(PlaceEntity);
            this.HTRepository.Commit();

            return AutoMapper.Mapper.Map<Place, PlaceModel>(PlaceEntity);
        }

        [HttpPost, Route("delete-language")]
        [HTAuthorize]
        public bool DeleteLanguage([FromBody]int Id)
        {
            var entity = this.HTRepository.PlaceLanguageRepository.GetAll()
                .FirstOrDefault(x => x.Id == Id);
            if (entity == null) return false;

            this.HTRepository.PlaceLanguageRepository.Delete(entity);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("filter")]
        [AllowAnonymous]
        public GetGridResponseModel<SearchPlaceModel> Filter([FromBody]GetGridRequestModel request)
        {
            var query = this.HTRepository.PlaceRepository.GetAll();
            query = query.Include("PlaceLanguages")
                .Include("Category.CategoryLanguages").Include("HTService.HTServiceLanguages")
                .Include(x => x.City).Include(x => x.District);

            var searchQuery = query.Select(q => new SearchPlaceModel
            {
                Id = q.Id,
                Title = q.PlaceLanguages.FirstOrDefault(t => t.Language == LanguageEnums.English).Title,
                CategoryId = q.CategoryId,
                CategoryName = q.Category.CategoryLanguages.FirstOrDefault(t => t.Language == LanguageEnums.English).Title,
                ServiceId = q.HTServiceId,
                ServiceName = q.HTServiceId != null ? q.HTService.HTServiceLanguages.FirstOrDefault(t => t.Language == LanguageEnums.English).Title : "",
                CityId = q.CityId,
                City = q.City.Name,
                DistrictId = q.DistrictId,
                District = q.District.Name,
                IsCategorySlider = q.IsCategorySlider,
                IsHomeSlider = q.IsHomeSlider,
                StartDate = q.StartDate,
                EndDate = q.EndDate,
                Rating = q.Rating,
                IsEvent = q.Category.IsEvent
            });

            // search
            if (!string.IsNullOrEmpty(request.Search))
            {
                var search = request.Search.ToLower().Trim();
                var searchInt = 0;
                var canParse = int.TryParse(search, out searchInt);

                searchQuery = searchQuery.Where(x => (x.City != null && x.City.ToLower().Contains(search))
                || (x.District != null && x.District.ToLower().Contains(search))
                || (x.Title != null && x.Title.ToLower().Contains(search))
                || (x.CategoryName != null && x.CategoryName.ToLower().Contains(search))
                || (x.ServiceName != null && x.ServiceName.ToLower().Contains(search))
                );
            }

            // filter
            if (request.Parameters != null)
            {
                foreach (var item in request.Parameters)
                {
                    var search = item.Value.ToString().ToLower().Trim();
                    switch (item.Key)
                    {
                        case "Name":
                            searchQuery = searchQuery.Where(x => x.Title != null && x.Title.ToLower().Contains(search));
                            break;
                        case "Category":
                            searchQuery = searchQuery.Where(x => x.CategoryName != null && x.CategoryName.ToLower().Contains(search));
                            break;
                        case "Service":
                            searchQuery = searchQuery.Where(x => x.ServiceName != null && x.ServiceName.ToLower().Contains(search));
                            break;
                        case "City":
                            searchQuery = searchQuery.Where(x => x.City != null && x.City.ToLower().Contains(search));
                            break;
                        case "District":
                            searchQuery = searchQuery.Where(x => x.District != null && x.District.ToLower().Contains(search));
                            break;
                        case "HomeHighlight":
                            if (search.Equals("0"))
                            {
                                searchQuery = searchQuery.Where(x => !x.IsHomeSlider.HasValue || !x.IsHomeSlider.Value);
                            }
                            else if (search.Equals("1"))
                            {
                                searchQuery = searchQuery.Where(x => x.IsHomeSlider.HasValue && x.IsHomeSlider.Value);
                            }
                            break;
                        case "CategoryHighlight":
                            if (search.Equals("0"))
                            {
                                searchQuery = searchQuery.Where(x => !x.IsCategorySlider.HasValue || !x.IsCategorySlider.Value);
                            }
                            else if (search.Equals("1"))
                            {
                                searchQuery = searchQuery.Where(x => x.IsCategorySlider.HasValue && x.IsCategorySlider.Value);
                            }
                            break;
                        case "IsEvent":
                            if (search.Equals("0"))
                            {
                                searchQuery = searchQuery.Where(x => !x.IsEvent.HasValue || !x.IsEvent.Value);
                            }
                            else if (search.Equals("1"))
                            {
                                searchQuery = searchQuery.Where(x => x.IsEvent.HasValue && x.IsEvent.Value);
                            }
                            break;
                        case "Ranking":
                            int searchInt = 0;
                            var canParse = int.TryParse(search, out searchInt);
                            searchQuery = searchQuery.Where(x => x.Rating == (float)searchInt);
                            break;
                        case "StartDate":
                            var json = JsonConvert.DeserializeObject<CompareModel>(search);
                            if (json != null)
                            {
                                var startTime0h = json.date.Value;
                                var endTime24h = json.date.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                                var comparator = json.comparator;
                                searchQuery = searchQuery.Where(x => x.StartDate.HasValue);
                                if (comparator.Equals("="))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value >= startTime0h && x.StartDate.Value <= endTime24h);
                                }
                                else if (comparator.Equals(">"))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value > endTime24h);
                                }
                                else if (comparator.Equals("<"))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value < startTime0h);
                                }
                                else if (comparator.Equals(">="))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value >= startTime0h);
                                }
                                else if (comparator.Equals("<="))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value <= endTime24h);
                                }
                                else if (comparator.Equals("!="))
                                {
                                    searchQuery = searchQuery.Where(x => x.StartDate.Value < startTime0h || x.StartDate.Value > endTime24h);
                                }
                            }
                            break;
                        case "EndDate":
                            var jsonD = JsonConvert.DeserializeObject<CompareModel>(search);
                            if (jsonD != null)
                            {
                                var startTime0h = jsonD.date.Value;
                                var endTime24h = jsonD.date.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                                var comparator = jsonD.comparator;
                                searchQuery = searchQuery.Where(x => x.EndDate.HasValue);
                                if (comparator.Equals("="))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value >= startTime0h && x.EndDate.Value <= endTime24h);
                                }
                                else if (comparator.Equals(">"))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value > endTime24h);
                                }
                                else if (comparator.Equals("<"))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value < startTime0h);
                                }
                                else if (comparator.Equals(">="))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value >= startTime0h);
                                }
                                else if (comparator.Equals("<="))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value <= endTime24h);
                                }
                                else if (comparator.Equals("!="))
                                {
                                    searchQuery = searchQuery.Where(x => x.EndDate.Value < startTime0h || x.EndDate.Value > endTime24h);
                                }
                            }
                            break;
                    }
                }
            }

            // sort
            switch (request.SortExpression)
            {
                case "Id":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.Id) : searchQuery.OrderByDescending(x => x.Id);
                    break;
                case "Name":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.Title) : searchQuery.OrderByDescending(x => x.Title);
                    break;
                case "Category":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.CategoryName) : searchQuery.OrderByDescending(x => x.CategoryName);
                    break;
                case "Service":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.ServiceName) : searchQuery.OrderByDescending(x => x.ServiceName);
                    break;
                case "City":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.City) : searchQuery.OrderByDescending(x => x.City);
                    break;
                case "District":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.District) : searchQuery.OrderByDescending(x => x.District);
                    break;
                case "HomeHighlight":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.IsHomeSlider) : searchQuery.OrderByDescending(x => x.IsHomeSlider);
                    break;
                case "CategoryHighlight":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.IsCategorySlider) : searchQuery.OrderByDescending(x => x.IsCategorySlider);
                    break;
                case "StartDate":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.StartDate) : searchQuery.OrderByDescending(x => x.StartDate);
                    break;
                case "EndDate":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.EndDate) : searchQuery.OrderByDescending(x => x.EndDate);
                    break;
                case "Ranking":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.Rating) : searchQuery.OrderByDescending(x => x.Rating);
                    break;
                case "IsEvent":
                    searchQuery = request.IsAsc ? searchQuery.OrderBy(x => x.IsEvent) : searchQuery.OrderByDescending(x => x.IsEvent);
                    break;
            }
            // count
            var totalRecord = searchQuery.Count();

            // take
            var models = searchQuery.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize).ToList();

            ////map
            //var models = entities.Select(x => AutoMapper.Mapper.Map<Place, PlaceModel>(
            //    x, opt =>
            //    {
            //        opt.AfterMap((ent, mod) =>
            //        {
            //            mod.Category = AutoMapper.Mapper.Map<CategoryModel>(ent.Category);
            //            mod.HTService = AutoMapper.Mapper.Map<HTServiceModel>(ent.HTService);
            //        });
            //    })).ToList();

            var response = new GetGridResponseModel<SearchPlaceModel>();
            response.DataSource = models;
            response.TotalRecord = totalRecord;
            return response;
        }

        [HttpGet, Route("gdn-search-place")]
        [AllowAnonymous]
        public List<AppSearchPlaceModel> GdnSearchPlace(string search, int? index)
        {
            var language = this.CurrentLanguage;
            var city = this.CurrentCityId;
            var currentId = index ?? 0;
            var itemsPerIndex = 20;

            if (string.IsNullOrEmpty(search) || search.Trim().Length < 3) return null;

            search = search.ToLower().Trim();

            var placeResult = new List<AppSearchPlaceModel>();
            if (language == LanguageEnums.Vietnamese)
            {
                var placeQuery = this.HTRepository.PlaceLanguageRepository.GetAll()
                .Include(pl => pl.Image)
                .Include(pl => pl.Place)
                .Include("Place.Category.CategoryLanguages")
                .Include("Place.HTService.HTServiceLanguages")
                .Include("Place.City")
                .Include("Place.District")
                .Where(pl => pl.Language == language && pl.Place != null && (pl.Place.CityId == city || 
                            (pl.Place.HTService != null && pl.Place.HTService.ShowInAllCity)));

                var placeList = placeQuery.ToList();
                search = search.StripDiacritics();
                var placeListRs = placeList.Where(pl => (pl.Place.City != null && pl.Place.City.Name.ToLower().StripDiacritics().Contains(search))
                || (pl.Place.District != null && pl.Place.District.Name.ToLower().StripDiacritics().Contains(search))
                || (pl.Place.Address != null && pl.Place.Address.ToLower().StripDiacritics().Contains(search))
                || (pl.Title != null && pl.Title.ToLower().StripDiacritics().Contains(search))
                || (pl.Place.Category != null && pl.Place.Category.CategoryLanguages.Any(cl => cl.Language == language && cl.Title.ToLower().StripDiacritics().Contains(search)))
                || (pl.Place.HTService != null && pl.Place.HTService.HTServiceLanguages.Any(sl => sl.Language == language && sl.Title.ToLower().StripDiacritics().Contains(search)))
                );
                placeResult = placeListRs
                .OrderByDescending(p => p.Place.IsHomeSlider)
                .ThenByDescending(p => p.Place.IsCategorySlider)
                .ThenByDescending(p => p.Place.CreatedDate)
                .Skip(currentId * itemsPerIndex).Take(itemsPerIndex)
                .ToList()
                .Select(p => new AppSearchPlaceModel
                {
                    Id = p.Place.Id,
                    Title = p.Title ?? string.Empty,
                    IsCategorySlider = p.Place.IsCategorySlider,
                    IsHomeSlider = p.Place.IsHomeSlider,
                    City = p.Place.City != null ? p.Place.City.Name : string.Empty,
                    District = p.Place.District != null ? p.Place.District.Name : string.Empty,
                    Address = p.Place.Address,
                    CoverImage = Mappers.Mapper.ToModel(p.Image),
                    Phone = p.Place.Phone,
                })
                .ToList();
            }
            else
            {
                var place = this.HTRepository.PlaceRepository.GetAll();
                var placeLanguage = this.HTRepository.PlaceLanguageRepository.GetAll();
                var category = this.HTRepository.CategoryRepository.GetAll();
                var categoryLanguage = this.HTRepository.CategoryLanguageRepository.GetAll();
                var service = this.HTRepository.HTServiceRepository.GetAll();
                var serviceLanguage = this.HTRepository.HTServiceLanguageRepository.GetAll();
                var image = this.HTRepository.ImageRepository.GetAll();
                var cities = this.HTRepository.CityRepository.GetAll();
                var district = this.HTRepository.DistrictRepository.GetAll();

                placeResult = (from pl in placeLanguage
                            join pimg in image on pl.ImageId equals pimg.Id into pimgs
                            from pimg in pimgs.DefaultIfEmpty()
                            join p in place on pl.PlaceId equals p.Id
                            join ci in cities on p.CityId equals ci.Id into cis
                            from ci in cis.DefaultIfEmpty()
                            join dis in district on p.DistrictId equals dis.Id into diss
                            from dis in diss.DefaultIfEmpty()
                            join c in category on p.CategoryId equals c.Id
                            join cl in categoryLanguage on c.Id equals cl.CategoryId
                            join s in service on p.HTServiceId equals s.Id into ss
                            from s in ss.DefaultIfEmpty()
                            where (p.CityId == CurrentCityId || (pl.Place.HTService != null && pl.Place.HTService.ShowInAllCity)) 
                                    && pl.Language == language &&
                                     cl.Language == language && s.HTServiceLanguages.Any(sl => sl.Language == language) &&
                                     ((ci != null && ci.Name.ToLower().Contains(search)) ||
                                     (dis != null && dis.Name.ToLower().Contains(search)) ||
                                     (p.Address != null && p.Address.ToLower().Contains(search)) ||
                                     (pl.Title != null && pl.Title.ToLower().Contains(search)) ||
                                     (cl.Title != null && cl.Title.ToLower().Contains(search)) ||
                                     (s != null && s.HTServiceLanguages.Any(sl => sl.Title != null && sl.Title.ToLower().Contains(search)))
                                     )
                            orderby c.IsEvent descending, p.IsHomeSlider descending, p.IsCategorySlider descending, p.Id descending
                            select new AppSearchPlaceModel
                            {
                                Id = p.Id,
                                Title = pl.Title ?? string.Empty,
                                City = ci != null ? ci.Name : string.Empty,
                                District = dis != null ? dis.Name : string.Empty,
                                Address = p.Address,
                                CoverImage = Mappers.Mapper.ToModel(pimg),
                                Phone = p.Phone,
                            })
                            .Skip(currentId * itemsPerIndex).Take(itemsPerIndex)
                            .ToList();
            }

            return placeResult;
        }

        [HttpGet, Route("gdn-get-favorite")]
        [AllowAnonymous]
        public List<AppFavoritePlaceModel> GdnGetFavorite(string favoriteIdStr)
        {
            var language = this.CurrentLanguage;
            var city = this.CurrentCityId;

            var favoriteIds = favoriteIdStr.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var ids = new List<int>();
            foreach (var idStr in favoriteIds)
            {
                var placeId = 0;
                if (int.TryParse(idStr, out placeId))
                {
                    ids.Add(placeId);
                }
            }
            var query = this.HTRepository.PlaceRepository.GetAll();
            if (ids.Count == 0) return null;
            var queryRs = query.Include("PlaceLanguages.Image").Include(x => x.City).Include(x => x.District)
                .Where(x => ids.Contains(x.Id)).ToList();

            var favoritePlaces = queryRs.Select(q => new AppFavoritePlaceModel
            {
                Id = q.Id,
                Title = q.PlaceLanguages.FirstOrDefault(t => t.Language == language) != null ? q.PlaceLanguages.FirstOrDefault(t => t.Language == language).Title : "",
                CoverImage = q.PlaceLanguages.FirstOrDefault(t => t.Language == language) != null ? Mappers.Mapper.ToModel(q.PlaceLanguages.FirstOrDefault(t => t.Language == language).Image) : null,
                Address = q.Address,
                District = q.District != null ? q.District.Name : null,
                Phone = q.Phone,
                City = q.City != null ? q.City.Name : null,
            }).ToList();
            return favoritePlaces;
        }

        [HttpGet, Route("get-place-by-id")]
        [AllowAnonymous]
        public PlaceModel GetPlaceById(int id)
        {
            if (id == 0)
            {
                var place = new Place();
                place.Id = 0;
                place.CreatedDate = Common.DateTimeHelper.GetDateTimeNow();
                place.UpdatedDate = Common.DateTimeHelper.GetDateTimeNow();

                place.PlaceLanguages = new List<PlaceLanguage>()
                {
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.English,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    },
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.Vietnamese,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    },
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.France,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    },
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.Chinese,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    },
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.Korean,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    },
                    new PlaceLanguage()
                    {
                        Title = "New Place",
                        Language = LanguageEnums.Japanese,
                        UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                        CreatedDate = DateTimeHelper.GetDateTimeNow(),
                    }
                };
                var placeModel = AutoMapper.Mapper.Map<Place, PlaceModel>(place);
                return placeModel;
            }
            var query = this.HTRepository.PlaceRepository.GetAll();
            var entity = query.Include("PlaceLanguages.Image").Include("PlaceLanguages.PlaceImages.Image").Include("PlaceLanguages.PlaceMoreInfo.Icon")
                .Include("Category.CategoryLanguages").Include("HTService.HTServiceLanguages")
                .Include(x => x.City).Include(x => x.District).FirstOrDefault(q => q.Id == id);
            foreach (var lang in entity.PlaceLanguages)
            {
                if (lang != null)
                {
                    lang.PlaceMoreInfo = lang.PlaceMoreInfo.OrderBy(i => i.Order).ToList();
                }
            }
            var model = AutoMapper.Mapper.Map<Place, PlaceModel>(
                entity, opt =>
                {
                    opt.AfterMap((ent, mod) =>
                    {
                        mod.Category = AutoMapper.Mapper.Map<CategoryModel>(ent.Category);
                        mod.HTService = AutoMapper.Mapper.Map<HTServiceModel>(ent.HTService);
                    });
                });

            return model;
        }

        [HttpPost, Route("save-imported-place")]
        [AllowAnonymous]
        public bool SaveImportedPlace([FromBody]List<ImportPlaceGroupModel> model)
        {
            this.HTRepository.GTDBUnitOfWork.DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            this.HTRepository.GTDBUnitOfWork.DbContext.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
            var enImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.English);
            if (enImportPlaceG == null) return false;
            var cateEntities = this.HTRepository.CategoryLanguageRepository.GetAll().ToList();
            var serviceEntities = this.HTRepository.HTServiceLanguageRepository.GetAll().ToList();
            var cityEntities = this.HTRepository.CityRepository.GetAll().ToList();
            var dicstrictEntities = this.HTRepository.DistrictRepository.GetAll().Include(t => t.City).ToList();
            var enImportPlaceNames = enImportPlaceG.ImportPlaces.Select(t => t.PlaceName).ToList();
            var placeEntities = this.HTRepository.PlaceRepository.GetAll()
                .Include("PlaceLanguages.Image")
                .Include("PlaceLanguages.PlaceImages")
                .Where(x => x.PlaceLanguages
                    .Any(p => p.Language == LanguageEnums.English && enImportPlaceNames.Contains(p.Title, StringComparer.CurrentCultureIgnoreCase)))
                .ToList();
            for (int i = 0; i < enImportPlaceG.ImportPlaces.Count; i++)
            {
                var enImportPlace = enImportPlaceG.ImportPlaces[i];

                var viImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.Vietnamese);
                var viImportPlace = viImportPlaceG != null && viImportPlaceG.ImportPlaces.Count > i ? viImportPlaceG.ImportPlaces[i] : null;

                var jaImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.Japanese);
                var jaImportPlace = jaImportPlaceG != null && jaImportPlaceG.ImportPlaces.Count > i ? jaImportPlaceG.ImportPlaces[i] : null;

                var chiImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.Chinese);
                var chiImportPlace = chiImportPlaceG != null && chiImportPlaceG.ImportPlaces.Count > i ? chiImportPlaceG.ImportPlaces[i] : null;

                var koImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.Korean);
                var koImportPlace = koImportPlaceG != null && koImportPlaceG.ImportPlaces.Count > i ? koImportPlaceG.ImportPlaces[i] : null;

                var frImportPlaceG = model.FirstOrDefault(t => t.Language == LanguageEnums.France);
                var frImportPlace = frImportPlaceG != null && frImportPlaceG.ImportPlaces.Count > i ? frImportPlaceG.ImportPlaces[i] : null;

                if (enImportPlace.PlaceInValid) continue;
                var placeEntity = placeEntities
                .FirstOrDefault(x => x.PlaceLanguages
                                .Any(p => p.Language == LanguageEnums.English && p.Title.Equals(enImportPlace.PlaceName, StringComparison.CurrentCultureIgnoreCase)));
                if (placeEntity == null) placeEntity = new Place();
                placeEntity.UpdatedDate = DateTimeHelper.GetDateTimeNow();
                placeEntity.Address = enImportPlace.Address;
                var city = cityEntities.FirstOrDefault(c => c.Name.Trim().ToLower() == enImportPlace.City.Trim().ToLower());
                placeEntity.CityId = city != null ? city.Id : (int?)null;
                placeEntity.CloseTime = DateTimeHelper.SafeParse(enImportPlace.CloseTime);
                var district = dicstrictEntities.FirstOrDefault(d => d.Name.Trim().ToLower() == enImportPlace.District.Trim().ToLower());
                placeEntity.DistrictId = district != null ? district.Id : (int?)null;
                placeEntity.IsCategorySlider = enImportPlace.IsCategorySlider;
                placeEntity.IsHomeSlider = enImportPlace.IsHomeSlider;
                placeEntity.OpenTime = DateTimeHelper.SafeParse(enImportPlace.OpenTime);
                placeEntity.Phone = enImportPlace.Phone;
                placeEntity.Fax = enImportPlace.Fax;
                placeEntity.Website = enImportPlace.Website;
                var cateE = cateEntities.FirstOrDefault(ca => ca.Language == LanguageEnums.English && ca.Title.ToLower() == enImportPlace.Category.ToLower());
                if (cateE != null)
                {
                    placeEntity.CategoryId = cateE.CategoryId;
                }
                var serviceE = serviceEntities.FirstOrDefault(s => s.Language == LanguageEnums.English && s.Title.ToLower() == enImportPlace.Service.ToLower());
                if (serviceE != null)
                {
                    placeEntity.HTServiceId = serviceE.HTServiceId;
                }

                this.HTRepository.PlaceRepository.Save(placeEntity);

                SaveImportLanguage(placeEntity, enImportPlace, LanguageEnums.English);
                SaveImportLanguage(placeEntity, viImportPlace, LanguageEnums.Vietnamese);
                SaveImportLanguage(placeEntity, jaImportPlace, LanguageEnums.Japanese);
                SaveImportLanguage(placeEntity, chiImportPlace, LanguageEnums.Chinese);
                SaveImportLanguage(placeEntity, koImportPlace, LanguageEnums.Korean);
                SaveImportLanguage(placeEntity, frImportPlace, LanguageEnums.France);
            }
            this.HTRepository.Commit();
            this.HTRepository.GTDBUnitOfWork.DbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            this.HTRepository.GTDBUnitOfWork.DbContext.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll;
            return true;
        }

        private void SaveImportLanguage(Place placeEntity, ImportPlaceModel importPlace, LanguageEnums lang)
        {
            if (importPlace == null) return;
            var placeLanguage = placeEntity.PlaceLanguages != null ? placeEntity.PlaceLanguages.FirstOrDefault(p => p.Language == lang) : null;
            if (placeLanguage == null)
            {
                placeLanguage = new PlaceLanguage();
            }
            placeLanguage.Title = importPlace.PlaceName;
            placeLanguage.Language = lang;
            if (placeLanguage.ImageId.HasValue && placeLanguage.ImageId != 0)
            {
                placeLanguage.Image.Url = importPlace.CoverImage;
            }
            else
            {
                placeLanguage.Image = new Image
                {
                    Url = importPlace.CoverImage
                };
            }
            placeLanguage.Description = importPlace.Description;
            placeLanguage.UpdatedDate = DateTimeHelper.GetDateTimeNow();
            placeLanguage.PlaceId = placeEntity.Id;
            this.HTRepository.PlaceLanguageRepository.Save(placeLanguage);
            if (!importPlace.PlaceImageError && importPlace.PlaceImages != null && importPlace.PlaceImages.Count > 0)
            {
                var imgs = importPlace.PlaceImages.Select(img => new Image
                {
                    Url = img
                }).Take(30).ToList();
                this.HTRepository.ImageRepository.Save(imgs);

                var placeImgs = placeLanguage.PlaceImages;
                if (placeImgs != null)
                {
                    this.HTRepository.PlaceImageRepository.Delete(placeImgs);
                }
                placeImgs = imgs.Select(t => new PlaceImage
                {
                    PlaceLangId = placeLanguage.Id,
                    ImageId = t.Id
                }).ToList();
                this.HTRepository.PlaceImageRepository.Save(placeImgs);
            }
            if (!importPlace.AdditionalInfoError && importPlace.AdditionalInfo != null && importPlace.AdditionalInfo.Count > 0)
            {
                var moreInfo = placeLanguage.PlaceMoreInfo;

                if (moreInfo != null)
                {
                    this.HTRepository.PlaceMoreInfoRepository.Delete(moreInfo);
                }

                moreInfo = importPlace.AdditionalInfo.Select((i, index) => new PlaceMoreInfo
                {
                    PlaceLangId = placeLanguage.Id,
                    Name = i.Key,
                    Value = i.Value,
                    Order = index
                }).ToList();

                this.HTRepository.PlaceMoreInfoRepository.Save(moreInfo);
            }
        }

        [HttpPost, Route("translate-place-language")]
        [AllowAnonymous]
        public PlaceLanguageModel TranslateServiceLanguage([FromBody]PlaceModel model)
        {
            if (model == null || model.PlaceLanguages == null || model.PlaceLanguages.Count != 2) return null;
            var entity = model.PlaceLanguages[0];
            if (entity == null) return null;
            var enPlaceLanguage = model.PlaceLanguages[1];
            if (enPlaceLanguage == null) return null;
            entity.Title = TranslateHelper.TranslateText(enPlaceLanguage.Title, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
            entity.Description = TranslateHelper.TranslateText(enPlaceLanguage.Description, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
            entity.ImageId = enPlaceLanguage.ImageId;
            entity.Image = enPlaceLanguage.Image;
            entity.PlaceImages = enPlaceLanguage.PlaceImages;
            if (enPlaceLanguage.PlaceMoreInfo != null)
            {
                entity.PlaceMoreInfo.Clear();
                entity.PlaceMoreInfo.AddRange(enPlaceLanguage.PlaceMoreInfo.Select(t => new PlaceMoreInfoModel
                {
                    Name = TranslateHelper.TranslateText(t.Name, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English)),
                    Value = TranslateHelper.TranslateText(t.Value, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English)),
                    IconId = t.IconId,
                    Icon = t.Icon,
                    IsHalf = t.IsHalf,
                    Order = t.Order,
                }).ToList());
            }
            return entity;
        }

        [HttpPost, Route("translate-all-place-language")]
        [AllowAnonymous]
        public PlaceModel TranslateAllServiceLanguage([FromBody]PlaceModel model)
        {
            if (model == null || model.PlaceLanguages == null) return null;
            var enPlaceLanguage = model.PlaceLanguages
                .FirstOrDefault(x => x.Language == LanguageEnums.English);
            if (enPlaceLanguage == null) return null;
            foreach (var entity in model.PlaceLanguages)
            {
                if (entity.Language == enPlaceLanguage.Language) continue;
                entity.Title = TranslateHelper.TranslateText(enPlaceLanguage.Title, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
                entity.Description = TranslateHelper.TranslateText(enPlaceLanguage.Description, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
                if (enPlaceLanguage.PlaceMoreInfo != null)
                {
                    entity.PlaceMoreInfo.Clear();
                    entity.PlaceMoreInfo.AddRange(enPlaceLanguage.PlaceMoreInfo.Select(t => new PlaceMoreInfoModel
                    {
                        Name = TranslateHelper.TranslateText(t.Name, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English)),
                        Value = TranslateHelper.TranslateText(t.Value, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English)),
                        IsHalf = t.IsHalf,
                        Order = t.Order,
                    }).ToList());
                }
            }
            return model;
        }
        [HttpGet, Route("download-template-high-level")]
        [AllowAnonymous]
        public FileResult DownloadTemplateHighLevel()
        {
            var filePath = Directory.GetCurrentDirectory() + @"\Resources\ImportTemplate.xlsx";
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(filePath));
            var response = File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ImportTemplate_" + DateTime.Now.Ticks + ".xlsx");
            return response;
        }

        [HttpGet, Route("app-get-place-by-id")]
        [AllowAnonymous]
        public AppPlaceModel AppGetPlaceById(int id)
        {
            var currentLang = this.CurrentLanguage;
            var currentCity = this.CurrentCityId;

            var place = this.HTRepository.PlaceRepository.GetAll();
            var placeLanguage = this.HTRepository.PlaceLanguageRepository.GetAll();
            var image = this.HTRepository.ImageRepository.GetAll();
            var city = this.HTRepository.CityRepository.GetAll();
            var district = this.HTRepository.DistrictRepository.GetAll();

            var rs = (from p in place
                      join ci in city on p.CityId equals ci.Id into cis
                      from ci in cis.DefaultIfEmpty()
                      join dis in district on p.DistrictId equals dis.Id into diss
                      from dis in diss.DefaultIfEmpty()
                      join pl in placeLanguage on p.Id equals pl.PlaceId
                      join pimg in image on pl.ImageId equals pimg.Id into pimgs
                      from pimg in pimgs.DefaultIfEmpty()
                      where p.Id == id && pl.Language == currentLang
                      select new AppPlaceModel
                      {
                          Id = p.Id,
                          ImageUrl = GetUrl(pimg),
                          Title = pl.Title,
                          Description = pl.Description,
                          Star = p.Rating,
                          Address = p.Address,
                          Phone = p.Phone,
                          Website = p.Website,
                          OpenTime = p.OpenTime,
                          CloseTime = p.CloseTime,
                          City = ci != null ? ci.Name : string.Empty,
                          District = dis != null ? dis.Name : string.Empty
                      }).FirstOrDefault();



            return rs;
        }

        [HttpGet, Route("app-get-place-images-by-id")]
        [AllowAnonymous]
        public List<AppPlaceImageModel> AppGetPlaceImagesById(int id)
        {
            var currentLang = this.CurrentLanguage;
            var currentCity = this.CurrentCityId;
            var placeLanguage = this.HTRepository.PlaceLanguageRepository.GetAll();
            var placeLanguageImage = this.HTRepository.PlaceImageRepository.GetAll();
            var image = this.HTRepository.ImageRepository.GetAll();
            var result = new List<AppPlaceImageModel>();

            result = (from pl in placeLanguage
                      join pli in placeLanguageImage on pl.Id equals pli.PlaceLangId
                      join img in image on pli.ImageId equals img.Id
                      where pl.PlaceId == id && pl.Language == LanguageEnums.English
                      select new AppPlaceImageModel()
                      {
                          Id = pli.Id,
                          ImageUrl = GetUrl(img)
                      }).ToList();

            return result;
        }

        [HttpGet, Route("app-get-place-info-by-id")]
        [AllowAnonymous]
        public List<PlaceMoreInfoModel> AppGetPlaceInfoById(int id)
        {
            var currentLang = this.CurrentLanguage;
            var currentCity = this.CurrentCityId;

            var placeMoreInfo = this.HTRepository.PlaceMoreInfoRepository.GetAll();
            var image = this.HTRepository.ImageRepository.GetAll();
            var result = new List<PlaceMoreInfoModel>();

            result = (from pm in placeMoreInfo
                      join img in image on pm.IconId equals img.Id into imgs
                      from img in imgs.DefaultIfEmpty()
                      where pm.PlaceLanguage.PlaceId == id && pm.PlaceLanguage.Language == currentLang
                      orderby pm.Order
                      select new PlaceMoreInfoModel()
                      {
                          IconId = pm.IconId,
                          Icon = AutoMapper.Mapper.Map<Image, ImageModel>(img),
                          Id = pm.Id,
                          IsHalf = pm.IsHalf,
                          Name = pm.Name,
                          Order = pm.Order,
                          PlaceLangId = pm.PlaceLangId,
                          Value = pm.Value,
                      }).ToList();

            return result;
        }

        [HttpGet, Route("get-nearby-place-by-id")]
        [AllowAnonymous]
        public List<AppPlaceModel> GetNearByPlaceById(int id)
        {
            var currentLang = this.CurrentLanguage;
            var currentCityId = this.CurrentCityId;

            var place = this.HTRepository.PlaceRepository.GetAll();
            var placeLanguage = this.HTRepository.PlaceLanguageRepository.GetAll();
            var image = this.HTRepository.ImageRepository.GetAll();
            var city = this.HTRepository.CityRepository.GetAll();
            var district = this.HTRepository.DistrictRepository.GetAll();

            var result = new List<AppPlaceModel>();

            var currentPlace = (from p in place
                                where p.Id == id
                                select p).FirstOrDefault();

            if (currentPlace == null) return result;

            var placesInCity = (from p in place
                                join ci in city on p.CityId equals ci.Id into cis
                                from ci in cis.DefaultIfEmpty()
                                join dis in district on p.DistrictId equals dis.Id into diss
                                from dis in diss.DefaultIfEmpty()
                                join pl in placeLanguage on p.Id equals pl.PlaceId
                                join pimg in image on pl.ImageId equals pimg.Id into pimgs
                                from pimg in pimgs.DefaultIfEmpty()
                                where p.CityId == currentCityId && p.Id != currentPlace.Id && pl.Language == currentLang
                                        && p.Latitude.HasValue && p.Longitude.HasValue
                                        && currentPlace.Latitude.HasValue && currentPlace.Longitude.HasValue
                                select new AppPlaceModel
                                {
                                    Id = p.Id,
                                    ImageUrl = GetUrl(pimg),
                                    Title = pl.Title,
                                    Phone = p.Phone,
                                    Description = pl.Description,
                                    Address = p.Address,
                                    OpenTime = p.OpenTime,
                                    CloseTime = p.CloseTime,
                                    City = ci != null ? ci.Name : string.Empty,
                                    District = dis != null ? dis.Name : string.Empty,
                                    Latitude = p.Latitude,
                                    Longitude = p.Longitude
                                }).ToList();
            if (placesInCity != null && placesInCity.Count > 0) {
                result = placesInCity.OrderBy(p => GetDistance(p.Latitude.Value, p.Longitude.Value, currentPlace.Latitude.Value, currentPlace.Longitude.Value)).Take(5).ToList();
            }
            
            return result;
        }

        [HttpGet, Route("cal-place-coordinate")]
        [HTAuthorize]
        public void CalPlaceCoordinate()
        {
            var place = this.HTRepository.PlaceRepository.GetAll().OrderByDescending(p => p.Id).Take(200).ToList();
            foreach (var p in place)
            {
                var city = p.CityId.HasValue ? this.HTRepository.CityRepository.GetAll().FirstOrDefault(c => c.Id == p.CityId) : null;
                var district = p.DistrictId.HasValue ? this.HTRepository.DistrictRepository.GetAll().FirstOrDefault(c => c.Id == p.DistrictId) : null;
                var fullAddress = GetFullAddress(p.Address, district != null ? district.Name : "", city != null ? city.Name : "");
                var location = GetLocationFromAddress(fullAddress).Result;
                if(location != null && (!p.Latitude.HasValue || !p.Longitude.HasValue))
                {
                    p.Latitude = location.Lat;
                    p.Longitude = location.Lng;
                    this.HTRepository.PlaceRepository.Save(p);
                }
            }
            this.HTRepository.Commit();
        }

        private string GetFullAddress(string address, string district, string city)
        {
            var fullAddress = "";
            if (string.IsNullOrEmpty(address) && string.IsNullOrEmpty(district) && string.IsNullOrEmpty(city)) return fullAddress;
            if (!string.IsNullOrEmpty(address))
                fullAddress += address;
            if (!string.IsNullOrEmpty(district))
                fullAddress += ", " + district;
            if (!string.IsNullOrEmpty(city))
                fullAddress += ", " + city;
            return fullAddress;
        }

        private async Task<Location> GetLocationFromAddress(string address)
        {
            try
            {
                string requestUrl = string.Format(@"{0}geocode/json?address={1}&key={2}", GGURL, address, GGKEY);
                WebRequest request = WebRequest.Create(requestUrl);

                WebResponse response = await request.GetResponseAsync();

                Stream data = response.GetResponseStream();

                StreamReader reader = new StreamReader(data);

                string responseFromServer = reader.ReadToEnd();

                JObject responseJson = JObject.Parse(responseFromServer);

                Location loc = new Location();

                if (responseJson["status"] != null && responseJson["status"].ToString() == "OK")
                {
                    string lat = responseJson["results"][0]["geometry"]["location"]["lat"].ToString();
                    string lng = responseJson["results"][0]["geometry"]["location"]["lng"].ToString();

                    decimal latd, lngd;
                    decimal.TryParse(lat, out latd);
                    decimal.TryParse(lng, out lngd);
                    loc.Lat = latd;
                    loc.Lng = lngd;
                    return loc;
                }
                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public double GetDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = DegToRad((double)(lat2 - lat1));  // deg2rad below
            var dLon = DegToRad((double)(lng2 - lng1));
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegToRad((double)lat1)) * Math.Cos(DegToRad((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                    ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        public double DegToRad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }

    class Location
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}
