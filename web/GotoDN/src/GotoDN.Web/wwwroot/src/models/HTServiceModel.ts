import {HTServiceLanguageModel} from "./HTServiceLanguageModel";
import {CategoryModel} from "./CategoryModel";

export interface HTServiceModel {
  Id: number,
  CreatedDate?: Date,
  UpdatedDate?: Date,
  Priority?: number,
  CategoryId?: number,
  Category?: CategoryModel,
  HTServiceLanguages?: HTServiceLanguageModel[]
}