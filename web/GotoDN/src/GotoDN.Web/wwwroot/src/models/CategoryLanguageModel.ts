import {CategoryModel} from "./CategoryModel";
import {ImageModel} from "./ImageModel";
import {LanguageEnums} from "../commons/constant";
export interface CategoryLanguageModel {
  Id: number,
  CategoryId?: number,
  Title?: string,
  IconId?: number,
  ImageId?: number,
  CreatedDate?: Date,
  UpdatedDate?: Date,
  Language?: LanguageEnums,
  Category?: CategoryModel,
  Image?: ImageModel,
  Icon?: ImageModel
}
