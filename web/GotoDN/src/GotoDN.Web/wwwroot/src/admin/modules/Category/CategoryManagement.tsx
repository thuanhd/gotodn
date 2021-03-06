import * as React from 'react';
import CategoryList from "../../components/CategoryManagement/CategoryList";
import {CategoryServiceInstance} from "../../services/CategoryService";
import CategoryDetail from "../../components/CategoryManagement/CategoryDetail";
import {CategoryModel} from "../../../models/CategoryModel";
import {LanguageEnums} from "../../../commons/constant";
import {CategoryLanguageModel} from "../../../models/CategoryLanguageModel";
import {SweetAlertResultEnums, SweetAlerts, SweetAlertTypeEnums} from "../../../commons/sweet-alerts";
interface thisState {
  Categories?: CategoryModel[],
  SelectedCategory?: CategoryModel,
  SelectedLanguage?: LanguageEnums
}
class CategoryManagement extends React.Component<{}, thisState> {
  state: thisState = {
    SelectedLanguage: LanguageEnums.English,
    Categories: [],
  };

  setState(state: thisState) {
    super.setState(state);
  }

  componentDidMount() {
    (async () => {
      this.setState({
        Categories: await CategoryServiceInstance.GetAll()
      });
    })();
  }

  private async createCategory() {
    let result = await CategoryServiceInstance.CreateCategory();
    if (result) {
      this.setState({
        SelectedCategory: result,
        SelectedLanguage: result.CategoryLanguages ? result.CategoryLanguages[0].Language : LanguageEnums.English,
      });
      this.forceUpdate();
    }
    else {
      window['notice_error']();
    }
  }

  private async updateCategory(model: CategoryModel) {
    let eng = model.CategoryLanguages.filter(x => x.Language == LanguageEnums.English)[0];
    let icon = eng.Icon;
    let img = eng.Image;
    model.CategoryLanguages.map( x => {
      if(x && x.Language != LanguageEnums.English)
      {
        x.Icon = icon;
        x.Image = img;
      }
    });
    let result = await CategoryServiceInstance.UpdateCategory(model);
    if (result) {
      this.setState({
        SelectedCategory: result,
        Categories: await CategoryServiceInstance.GetAll()
      });
      window['notice']('success-notice', 'Thành công', 'Đã lưu dữ liệu thành công.', 'fa fa-check-circle-o');
    }
  }

  private async deleteCategory(Id: number) {
    if (await SweetAlerts.show({
        type: SweetAlertTypeEnums.Warning,
        title: 'Xác nhận xóa',
        text: 'Bạn có chắc muốn xóa danh mục này?',
        showCancelButton: true,
        confirmButtonText: 'Đồng ý xóa',
        closeOnConfirm: true
      }) == SweetAlertResultEnums.Confirm) {
      let result = await CategoryServiceInstance.DeleteCategory(Id);
      if (result) {
        window['notice_delete_success']();
        this.setState({
          Categories: this.state.Categories.filter(x => x.Id != Id),
        });
        if(!this.state.SelectedCategory || this.state.SelectedCategory.Id == Id) {
          this.setState({
            SelectedCategory: null,
            SelectedLanguage: null
          });
        }
      }
      else {
        window['notice']('error-notice', 'Lỗi', 'Không thể xóa được bản ghi vì bản ghi được sử dụng trong hệ thống, bạn chỉ có thể xóa được bản ghi nếu nó không được sử dụng trong hệ thống.', 'glyphicon glyphicon-remove');
      }
    }
  }

  private async addCategoryLanguage(lang: LanguageEnums) {
    if(lang == LanguageEnums.All) {
      let result = await CategoryServiceInstance.AddAllLanguage(this.state.SelectedCategory.Id);
      if (result) {
        window['notice_create_success']();
        this.setState({SelectedCategory: result});
      }
      else {
        window['notice_error']();
      }
    }
    else {
      let categoryLanguage: CategoryLanguageModel = {
        Id: 0,
        Title: "",
        CategoryId: this.state.SelectedCategory.Id,
        Language: lang,
      };

      let result = await CategoryServiceInstance.AddLanguage(categoryLanguage);
      if (result) {
        window['notice_create_success']();
        this.state.SelectedCategory.CategoryLanguages.push(result);
        this.setState({
          SelectedLanguage: lang,
        });
      }
      else {
        window['notice_error']();
      }
    }
  }

  private async deleteCategoryLanguage(Id: number) {debugger;
    if (await SweetAlerts.show({
        type: SweetAlertTypeEnums.Warning,
        title: 'Xác nhận xóa',
        text: 'Bạn có chắc muốn xóa ngôn ngữ này?',
        showCancelButton: true,
        confirmButtonText: 'Đồng ý xóa',
        closeOnConfirm: true
      }) == SweetAlertResultEnums.Confirm) {
      let result = await CategoryServiceInstance.DeleteLanguage(Id);
      if (result) {
        window['notice_delete_success']();
        this.state.SelectedCategory.CategoryLanguages = this.state.SelectedCategory.CategoryLanguages
          .filter(x => x.Id != Id);
        this.setState({SelectedLanguage: LanguageEnums.English})
        this.forceUpdate();
      }
      else {
        window['notice_error']();
      }
    }
  }

  render() {
    return (
      <div className="page-content-wrapper">
        <div className={`page-content-inner`}>
          <div id="page-header" className="clearfix">
            <div className="page-header">
              <h2>Danh mục</h2>
              <span className="txt">Quản lý danh mục</span>
            </div>
            <div className="header-stats">
            </div>
          </div>
          <div className="row">
            <div className="col-lg-12 col-md-12 col-sm-12">
              <div className="panel panel-default plain toggle panelMove">
                <div className="panel-body">
                  <CategoryList Categories={this.state.Categories}
                                SelectedCategory={this.state.SelectedCategory}
                                ChangeSelectedCategory={(model) => this.setState({
                                  SelectedCategory: model,
                                  SelectedLanguage: LanguageEnums.English,
                                })}
                                OnChangeOrder={(models) => this.setState({
                                  Categories: models
                                })}
                                DeleteCategory={(Id: number) => this.deleteCategory(Id)}
                                CreateCategory={() => this.createCategory()}
                  />
                  <CategoryDetail SelectedCategory={this.state.SelectedCategory}
                                  SelectedLanguage={this.state.SelectedLanguage}
                                  ChangeSelectedLanguage={(language) => this.setState({
                                    SelectedLanguage: language
                                  })}
                                  cancelCategory={() => {
                                     this.setState({
                                        SelectedLanguage: LanguageEnums.English,
                                        SelectedCategory: null,
                                     })
                                   }}
                                  OnCategoryLanguageChange={(obj: CategoryLanguageModel) => {
                                    for (let i = 0; i < this.state.SelectedCategory.CategoryLanguages.length; i++) {
                                      if (this.state.SelectedCategory.CategoryLanguages[i].Language == obj.Language) {
                                        this.state.SelectedCategory.CategoryLanguages[i] = obj;
                                        break;
                                      }
                                    }
                                    this.forceUpdate();
                                  }}
                                  SaveCategory={(model) => this.updateCategory(model)}
                                  DeleteCategory={(Id: number) => this.deleteCategory(Id)}
                                  AddCategoryLanguage={(lang: LanguageEnums) => this.addCategoryLanguage(lang)}
                                  DeleteCategoryLanguage={(Id: number) => this.deleteCategoryLanguage(Id)}
                                  ChangeEvent={(check: boolean) => {
                                    this.state.SelectedCategory.IsEvent = check;
                                    this.forceUpdate();
                                  }}
                                  ChangeGovernment={(check: boolean) => {
                                    this.state.SelectedCategory.IsGovernment = check;
                                    this.forceUpdate();
                                  }}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}


export default CategoryManagement;