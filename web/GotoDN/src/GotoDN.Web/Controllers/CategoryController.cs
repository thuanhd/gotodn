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
using Google.Cloud.Translation.V2;
using Google.Apis.Services;
using Google.Apis.Translate.v2;
using Google.Apis.Translate.v2.Data;
using TranslationsResource = Google.Apis.Translate.v2.Data.TranslationsResource;

namespace GotoDN.Web.Controllers
{
    [Route("category")]
    public class CategoryController : BaseController
    {
        public CategoryController(HTRepository repository) : base(repository)
        {
        }

        [HttpGet, Route("get-all")]
        [AllowAnonymous]
        public List<CategoryModel> GetAll()
        {
            var currentLanguage = LanguageEnums.Korean;

            var entities = this.HTRepository.CategoryRepository.GetAll()
                .Include("CategoryLanguages.Image")
                .Include("CategoryLanguages.Icon")
                .Include(c => c.HTServices)
                .Take(1000).OrderBy(t => t.Order).ToList();

            var lang = this.CurrentLanguage;
            var city = this.CurrentCityId;

            var models = entities.Select(x => AutoMapper.Mapper.Map<Category, CategoryModel>(x)).ToList();

            return models;
        }

        [HttpPost, Route("create-category")]
        [HTAuthorize]
        public CategoryModel CreateCategory()
        {
            var entity = new Category();
            entity.Id = 0;
            entity.CreatedDate = Common.DateTimeHelper.GetDateTimeNow();
            entity.UpdatedDate = Common.DateTimeHelper.GetDateTimeNow();
            entity.CategoryLanguages = new List<CategoryLanguage>()
            {
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.English,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.Vietnamese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.France,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.Chinese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.Korean,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
                new CategoryLanguage()
                {
                    Title = "New Category",
                    Language = LanguageEnums.Japanese,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    CreatedDate = DateTimeHelper.GetDateTimeNow(),
                },
            };

            //this.HTRepository.CategoryRepository.Save(entity);
            //this.HTRepository.Commit();
            return AutoMapper.Mapper.Map<Category, CategoryModel>(entity);
        }

        [HttpPost, Route("delete-category")]
        [HTAuthorize]
        public bool DeleteCategory([FromBody]int Id)
        {
            var entity = this.HTRepository.CategoryRepository.GetAll()
                .Include(c => c.HTServices)
                .Include(c => c.Places)
                .FirstOrDefault(x => x.Id == Id);
            if (entity == null ||
                (entity.HTServices != null && entity.HTServices.Count > 0) ||
                (entity.Places != null && entity.Places.Count > 0)
                ) return false;
            this.HTRepository.CategoryRepository.Delete(entity);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("update-category")]
        [HTAuthorize]
        public CategoryModel UpdateCategory([FromBody]CategoryModel model)
        {
            if (model == null) return null;
            var entity = this.HTRepository.CategoryRepository.GetAll()
                .Include("CategoryLanguages.Image")
                .Include("CategoryLanguages.Icon")
                .FirstOrDefault(x => x.Id == model.Id);
            if (entity == null)
            {
                entity = new Category();
            };
            entity.UpdatedDate = DateTimeHelper.GetDateTimeNow();
            entity.IsEvent = model.IsEvent;
            entity.IsGovernment = model.IsGovernment;
            if (entity.CategoryLanguages == null || entity.CategoryLanguages.Count == 0)
            {
                entity.CategoryLanguages = model.CategoryLanguages.Select(c => new CategoryLanguage
                {
                    Title = c.Title,
                    ImageId = c.Image != null ? c.Image.Id : (int?)null,
                    IconId = c.Icon != null ? c.Icon.Id : (int?)null,
                    UpdatedDate = DateTimeHelper.GetDateTimeNow(),
                    Language = c.Language
                }).ToList();
            }
            else
            {
                foreach (var item in entity.CategoryLanguages)
                {
                    var en = model.CategoryLanguages.FirstOrDefault(x => x.Id == item.Id);
                    if (en != null)
                    {
                        item.Title = en.Title;
                        item.ImageId = en.Image != null ? en.Image.Id : (int?)null;
                        item.IconId = en.Icon != null ? en.Icon.Id : (int?)null;
                        item.UpdatedDate = DateTimeHelper.GetDateTimeNow();
                        item.Language = en.Language;
                    }
                }
            }

            this.HTRepository.CategoryRepository.Save(entity);
            this.HTRepository.Commit();
            if (model.Id == 0) model.Id = entity.Id;
            return model;
        }

        [HttpPost, Route("add-language")]
        [HTAuthorize]
        public CategoryLanguageModel AddLanguage([FromBody]CategoryLanguageModel model)
        {
            if (model == null) return null;
            var CatEntity = this.HTRepository.CategoryRepository.GetAll()
                .FirstOrDefault(x => x.Id == model.CategoryId.GetValueOrDefault());
            if (CatEntity == null) return null;
            if (CatEntity.CategoryLanguages == null) CatEntity.CategoryLanguages = new List<CategoryLanguage>();

            var LangEntity = new CategoryLanguage();
            LangEntity.CategoryId = model.CategoryId;
            LangEntity.Language = model.Language;
            LangEntity.Title = model.Title;
            LangEntity.UpdatedDate = DateTimeHelper.GetDateTimeNow();
            LangEntity.CreatedDate = DateTimeHelper.GetDateTimeNow();

            CatEntity.CategoryLanguages.Add(LangEntity);
            this.HTRepository.CategoryRepository.Save(CatEntity);
            this.HTRepository.Commit();

            return AutoMapper.Mapper.Map<CategoryLanguage, CategoryLanguageModel>(LangEntity);
        }

        [HttpPost, Route("delete-language")]
        [HTAuthorize]
        public bool DeleteLanguage([FromBody]int Id)
        {
            var entity = this.HTRepository.CategoryLanguageRepository.GetAll()
                .FirstOrDefault(x => x.Id == Id);
            if (entity == null) return false;

            this.HTRepository.CategoryLanguageRepository.Delete(entity);
            this.HTRepository.Commit();
            return true;
        }

        [HttpPost, Route("translate-category-language")]
        [AllowAnonymous]
        public CategoryLanguageModel TranslateCategoryLanguage([FromBody]CategoryModel model)
        {
            if (model == null || model.CategoryLanguages == null || model.CategoryLanguages.Count != 2) return null;
            var entity = model.CategoryLanguages[0];
            if (entity == null) return null;
            var enCateLanguage = model.CategoryLanguages[1];
            if (enCateLanguage == null) return null;
            entity.Title = TranslateHelper.TranslateText(enCateLanguage.Title, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
            entity.ImageId = enCateLanguage.ImageId;
            entity.Image = enCateLanguage.Image;
            entity.IconId = enCateLanguage.IconId;
            entity.Icon = enCateLanguage.Icon;
            return entity;
        }

        [HttpPost, Route("translate-all-category-language")]
        [AllowAnonymous]
        public CategoryModel TranslateAllCategoryLanguage([FromBody]CategoryModel model)
        {
            if (model == null || model.CategoryLanguages == null) return null;
            var enCateLanguage = model.CategoryLanguages
                .FirstOrDefault(x => x.Language == LanguageEnums.English);
            if (enCateLanguage == null) return null;
            foreach (var entity in model.CategoryLanguages)
            {
                entity.Title = TranslateHelper.TranslateText(enCateLanguage.Title, TranslateHelper.GetLanguageCode(entity.Language ?? LanguageEnums.English));
                entity.ImageId = enCateLanguage.ImageId;
                entity.Image = enCateLanguage.Image;
                entity.IconId = enCateLanguage.IconId;
                entity.Icon = enCateLanguage.Icon;
            }
            return model;
        }

        [HttpGet, Route("get-category-slider")]
        [AllowAnonymous]
        public List<SliderModel> GetCategorySlider()
        {
            var currentLanguage = this.CurrentLanguage;
            var currentCityId = this.CurrentCityId;

            var result = new List<SliderModel>();
            var today = DateTimeHelper.GetDateTimeNow();

            var eventPlaces = this.HTRepository.PlaceRepository.GetAll()
                .Where(x => x.CityId == currentCityId && (
                (x.Category != null && x.Category.IsEvent.HasValue && x.Category.IsEvent.Value
                && x.StartDate <= today.Date && x.EndDate > today.Date) ||
                (x.IsCategorySlider.HasValue && x.IsCategorySlider.Value)) &&
                x.PlaceLanguages.Any(p => p.Language == currentLanguage))
                .Include("PlaceLanguages.Image").Include(p => p.Category).ToList();

            if (eventPlaces == null) return null;
            result = eventPlaces.Select(x =>
                new SliderModel()
                {
                    Id = x.Id,
                    SubTitle = x.PlaceLanguages.Where(z => z.Language == currentLanguage).FirstOrDefault().Description,
                    Title = x.PlaceLanguages.Where(z => z.Language == currentLanguage).FirstOrDefault().Title,
                    Url = x.PlaceLanguages.Where(z => z.Language == currentLanguage).FirstOrDefault().Image != null ?
                        GetUrl(x.PlaceLanguages.Where(z => z.Language == currentLanguage).FirstOrDefault().Image) : Common.DefaultPhoto.ImageUrl,
                    CreateDate = x.CreatedDate,
                    IsEvent = x.Category != null ? x.Category.IsEvent : null,
                    IsCategorySlider = x.IsCategorySlider,
                }).ToList().OrderByDescending(t => t.IsEvent).ThenBy(t => t.IsCategorySlider).ThenByDescending(t => t.CreateDate).Take(20).ToList();

            return result;
        }

        [HttpGet, Route("get-category-data")]
        [AllowAnonymous]
        public List<MenuListModel> GetCategoryData()
        {
            var currentLang = this.CurrentLanguage;

            var result = new List<MenuListModel>();

            var category = this.HTRepository.CategoryRepository.GetAll()
                .Include("CategoryLanguages.Image")
                .Include("CategoryLanguages.Icon")
                .Include("HTServices.HTServiceLanguages.Image")
                .Where(c => c.CategoryLanguages.Any(z => z.Language == currentLang))
                .ToList();

            result = category.Select(x =>
                new MenuListModel()
                {
                    Id = x.Id,
                    Order = x.Order,
                    Name = x.CategoryLanguages.FirstOrDefault(z => z.Language == currentLang).Title,
                    Image = x.CategoryLanguages.FirstOrDefault(z => z.Language == currentLang).Image != null ?
                            GetUrl(x.CategoryLanguages.FirstOrDefault(z => z.Language == currentLang).Image) : null,
                    Icon = x.CategoryLanguages.FirstOrDefault(z => z.Language == currentLang).Icon != null ?
                            GetUrl(x.CategoryLanguages.FirstOrDefault(z => z.Language == currentLang).Icon) : null,
                    Items = x.HTServices.Count > 0 ?
                        x.HTServices.Select(y => new MenuItemModel()
                        {
                            Id = y.Id,
                            Title = y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang) != null ?
                                    y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang).Title : "",
                            Url = y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang) != null &&
                                    y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang).Image != null ?
                                    GetUrl(y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang).Image) : null
                        }).ToList()
                      : null
                }).Take(100).ToList();
            
            result = result.OrderBy(x => x.Order).ToList();
            return result;
        }

        [HttpPost, Route("order-category")]
        [AllowAnonymous]
        public List<CategoryModel> OrderCategory([FromBody]List<int> categoryIds)
        {
            for (int i = 0; i < categoryIds.Count; i++)
            {
                var cateEntity = this.HTRepository.CategoryRepository.GetObject(categoryIds[i]);
                if (cateEntity != null)
                {
                    cateEntity.Order = (i + 1);
                    this.HTRepository.CategoryRepository.Save(cateEntity);
                }
            }
            this.HTRepository.Commit();
            var entities = this.HTRepository.CategoryRepository.GetAll()
                .Include("CategoryLanguages.Image")
                .Include("CategoryLanguages.Icon")
                .Take(1000).OrderBy(t => t.Order).ToList();

            var models = entities.Select(x => AutoMapper.Mapper.Map<Category, CategoryModel>(x)).ToList();

            return models;
        }


        [HttpGet, Route("get-category-by-id")]
        [AllowAnonymous]
        public MenuListModel GetCategoryById(int Id)
        {
            var currentLang = this.CurrentLanguage;
            var city = this.CurrentCityId;

            var result = new MenuListModel();
            var CategoryEntity = this.HTRepository.CategoryLanguageRepository.GetAll()
                .Include(x => x.Icon).Include("Category.HTServices.HTServiceLanguages.Image")
                .FirstOrDefault(x => x.CategoryId == Id && x.Language == currentLang);
            if (CategoryEntity == null) return result;

            result.Id = CategoryEntity.CategoryId.Value;
            result.Icon = CategoryEntity.Icon != null ? GetUrl(CategoryEntity.Icon) : "";
            result.Name = CategoryEntity.Title;
            result.Items = CategoryEntity.Category.HTServices.Select(y => new MenuItemModel()
            {
                Id = y.Id,
                Title = y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang).Title,
                Url = y.HTServiceLanguages.FirstOrDefault(z => z.Language == currentLang).Image != null ? GetUrl(y.HTServiceLanguages.Where(z => z.Language == currentLang).FirstOrDefault().Image) : Common.DefaultPhoto.ImageUrl,
            }).ToList();

            return result;
        }

        [HttpGet, Route("get-category-no-service-by-id")]
        [AllowAnonymous]
        public CategoryModel GetCategoryNoServiceById(int id)
        {
            var currentLang = this.CurrentLanguage;
            var category = this.HTRepository.CategoryRepository.GetAll()
                .Include("Places.PlaceLanguages.Image")
                .Include(c => c.CategoryLanguages)
                .Where(x => x.Places.Any(p => p.PlaceLanguages.Any(l => l.Language == currentLang)) 
                && x.Places.All(p => p.HTServiceId == null || p.HTServiceId == 0) && x.Id == id).FirstOrDefault();
            if (category == null) return null;
            return AutoMapper.Mapper.Map<CategoryModel>(category);
        }


    }
}
