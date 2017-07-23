﻿using GotoDN.Repository;
using GotoDN.Web.Authentication;
using GotoDN.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GotoDN.Web.Controllers
{
    [Route("image")]
    public class ImageController : BaseController
    {
        public ImageController(HTRepository repository) : base(repository) { }

        [Route("upload-new-on-home-image"), HttpPost]
        [AllowAnonymous]
        public async Task<ImageModel> UploadNewOnHomeImage(IFormFile file)
        {
            if (file == null) return null;
            var stream = file.OpenReadStream();
            var fileKey = string.Format("home/img/home_{0}.png", Guid.NewGuid().ToString());

            var image = await this.CreateNewImage(stream, fileKey);

            return Mappers.Mapper.ToModel(image);
        }
        [Route("upload-new-image-for-description"), HttpPost]
        [AllowAnonymous]
        public async Task<ImageModel> UploadNewImageForDescription(IFormFile file)
        {
            if (file == null) return null;
            var stream = file.OpenReadStream();
            var fileKey = string.Format("desc/img/desc_{0}.png", Guid.NewGuid().ToString());

            var image = await this.CreateNewImage(stream, fileKey);

            return Mappers.Mapper.ToModel(image);
        }

        [Route("upload-new-image"), HttpPost]
        [AllowAnonymous]
        public async Task<ImageModel> UploadNewImageForCommon(IFormFile file)
        {
            if (file == null) return null;
            var stream = file.OpenReadStream();
            var fileKey = string.Format("coms/img/coms_{0}.jpg", Guid.NewGuid().ToString());
            Stream compressStream = new MemoryStream();
            compressStream = ImageHelper.CompressImage(stream, 80);
            var image = await this.CreateNewImage(compressStream, fileKey);
            return Mappers.Mapper.ToModel(image);
        }

        [Route("upload-new-icon"), HttpPost]
        [AllowAnonymous]
        public async Task<ImageModel> UploadNewIcon(IFormFile file)
        {
            if (file == null) return null;
            var stream = file.OpenReadStream();
            var fileKey = string.Format("coms/img/coms_{0}.png", Guid.NewGuid().ToString());
            Stream compressStream = new MemoryStream();
            compressStream = ImageHelper.ScaleIcon(stream, 256, 256);
            var image = await this.CreateNewImage(compressStream, fileKey);
            return Mappers.Mapper.ToModel(image);
        }

        [Route("import-excel-high-level"), HttpPost]
        [AllowAnonymous]
        public List<ImportPlaceGroupModel> ImportExcelHighLevel(IFormFile file)
        {
            if (file == null) return null;
            var stream = file.OpenReadStream();
            var package = new ExcelPackage(stream);
            var importedPlaces = new List<ImportPlaceModel>();
            var cateEntities = this.HTRepository.CategoryLanguageRepository.GetAll();
            var serviceEntities = this.HTRepository.HTServiceLanguageRepository.GetAll();
            var cityEntities = this.HTRepository.CityRepository.GetAll();
            var dicstrictEntities = this.HTRepository.DistrictRepository.GetAll().Include(t => t.City);
            try
            {
                for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
                {
                    var currentLang = ExcelHelper.Languages[i];
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[currentLang];

                    for (int r = 2; r <= workSheet.Dimension.End.Row; r++)
                    {
                        var iPlace = new ImportPlaceModel();
                        #region Read & Verify data
                        iPlace.Language = ExcelHelper.GetLangEnums(currentLang);
                        int c = workSheet.Dimension.Start.Column;
                        if (string.IsNullOrWhiteSpace((string)workSheet.Cells[r, c].Value))
                            continue;
                        if (c <= workSheet.Dimension.End.Column)
                            iPlace.PlaceName = workSheet.Cells[r, c++].Value.ToString();
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            var cate = workSheet.Cells[r, c++].Value.ToString();
                            var cateE = cateEntities.FirstOrDefault(ca => ca.Title.ToLower() == cate.ToLower());
                            if (cateE != null)
                            {
                                var category = this.HTRepository.CategoryRepository.GetAll().FirstOrDefault(t => t.Id == cateE.CategoryId);
                                var categoryLang = category.CategoryLanguages.FirstOrDefault(t => t.Language == ExcelHelper.GetLangEnums(currentLang));
                                if (categoryLang != null)
                                    iPlace.Category = categoryLang.Title;
                                else
                                {
                                    iPlace.Category = cate;
                                    iPlace.CategoryNotExist = true;
                                }
                            }
                            else
                            {
                                iPlace.Category = cate;
                                iPlace.CategoryNotExist = true;
                            }
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            var serv = workSheet.Cells[r, c++].Value.ToString();
                            var serviceE = serviceEntities.FirstOrDefault(s => s.Title.ToLower() == serv.ToLower());
                            if (serviceE != null)
                            {
                                var service = this.HTRepository.HTServiceRepository.GetAll().FirstOrDefault(t => t.Id == serviceE.HTServiceId);
                                var serviceLang = service.HTServiceLanguages.FirstOrDefault(t => t.Language == ExcelHelper.GetLangEnums(currentLang));
                                if (serviceLang != null)
                                    iPlace.Service = serviceLang.Title;
                                else
                                {
                                    iPlace.Service = serv;
                                    iPlace.ServiceNotExist = true;
                                }
                            }
                            else
                            {
                                iPlace.Service = serv;
                                iPlace.ServiceNotExist = true;
                            }
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.Description = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.CoverImage = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.IsHomeSlider = workSheet.Cells[r, c++].Value.ToString() == "Yes";
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.IsCategorySlider = workSheet.Cells[r, c++].Value.ToString() == "Yes";
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.City = workSheet.Cells[r, c].Value.ToString();
                            if (!cityEntities.Any(t => t.Name.ToLower() == workSheet.Cells[r, c].Value.ToString().Trim().ToLower()))
                                iPlace.CityNotExist = true;
                            c++;
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.District = workSheet.Cells[r, c].Value.ToString();
                            if (!(dicstrictEntities.Any(t => t.Name.ToLower() == workSheet.Cells[r, c].Value.ToString().Trim().ToLower() && t.City.Name.ToLower() == iPlace.City.ToLower())))
                                iPlace.DistrictNotExist = true;
                            c++;
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.Address = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.Phone = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.Fax = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.OpenTime = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.CloseTime = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            iPlace.Website = workSheet.Cells[r, c++].Value.ToString();
                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            try
                            {
                                var additionalInfoValue = workSheet.Cells[r, c++].Value.ToString();
                                iPlace.AdditionalInfo = additionalInfoValue.Split('\n').Select(t => new KeyValuePair<string, string>(t.Trim().Split(':').ToList()[0].Trim(), t.Trim().Split(':').ToList()[1].Trim())).ToList();
                            }
                            catch
                            {
                                iPlace.AdditionalInfoError = true;
                            }

                        }
                        if (c <= workSheet.Dimension.End.Column)
                        {
                            try
                            {
                                var imagesValue = workSheet.Cells[r, c++].Value.ToString();
                                iPlace.PlaceImages = imagesValue.Split('\n').Select(t => t.Trim()).ToList();
                            }
                            catch
                            {
                                iPlace.PlaceImageError = true;
                            }
                        }
                        #endregion
                        importedPlaces.Add(iPlace);
                    }
                }
            }
            catch
            {
                return null;
            }

            var placeGroup = new List<ImportPlaceGroupModel>();
            placeGroup = importedPlaces.GroupBy(t => t.Language).Select(t => new ImportPlaceGroupModel
            {
                Language = t.Key,
                ImportPlaces = t.ToList(),
            }).ToList();

            return placeGroup;
        }
    }
}
