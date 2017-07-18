import * as React from 'react';
import {HTServiceModel} from "../../../models/HTServiceModel";
import {LanguageEnums} from "../../../commons/constant";
import ServiceLanguageDetail from "./ServiceLanguageDetail";
import {HTServiceLanguageModel} from "../../../models/HTServiceLanguageModel";
import {ComboBox, ReactSelectModel} from "../ComboBox/ComboBox";
import {CategoryModel} from "../../../models/CategoryModel";
interface thisProps {
  SelectedHTService: HTServiceModel,
  SelectedLanguage: LanguageEnums,
  ChangeSelectedLanguage: (l: LanguageEnums) => void,
  OnHTServiceLanguageChange: (destination: HTServiceLanguageModel) => void,
  SaveHTService: () => void,
  DeleteHTService: (Id: number) => void,
  AddHTServiceLanguage: (lang: LanguageEnums) => void,
  DeleteHTServiceLanguage: (Id: number) => void,
  Categories: CategoryModel[],
  ClickSlectCategory: (Id) => void,
}

class HTServiceDetail extends React.Component<thisProps, {}> {

  render() {
    debugger;
    let languages: { Language: LanguageEnums, Title: string }[] = [
      {Language: LanguageEnums.Vietnamese, Title: 'Tiếng Việt'},
      {Language: LanguageEnums.English, Title: 'Tiếng Anh'},
      {Language: LanguageEnums.France, Title: 'Tiếng Pháp'},
      {Language: LanguageEnums.Chinese, Title: 'Tiếng Trung'},
      {Language: LanguageEnums.Japanese, Title: 'Tiếng Nhật'},
      {Language: LanguageEnums.Korean, Title: 'Tiếng Hàn'},
    ];
    let Categories: ReactSelectModel[];
    if(this.props.Categories && this.props.Categories.length > 0){
      Categories = this.props.Categories.map(
        x => {return {label: x.CategoryLanguages ? x.CategoryLanguages[0].Title : "", value: x.Id}}
      );
    }

    return (
      <div className="col-lg-8 cate-right-form" style={{marginBottom: 150}}>
        <h3>Xem thông tin chi tiết dịch vụ</h3>
        <hr/>
        {this.props.SelectedHTService != null ?
          <div className="col-lg-12 col-sm-12 form-horizontal">
            <div className="tabs mb20">
              <ul className="nav nav-tabs">
                {
                  this.props.SelectedHTService.HTServiceLanguages.map(x =>
                    <li key={x.Id}
                        className={(this.props.SelectedLanguage || LanguageEnums.English) == x.Language ? 'active' : ''}>
                      <a onClick={() => this.props.ChangeSelectedLanguage(x.Language)}>
                        {languages.filter(r => r.Language == x.Language)[0].Title}
                        &nbsp;
                        &nbsp;
                        {x.Language == LanguageEnums.Vietnamese ?
                          null : <span onClick={() => this.props.DeleteHTServiceLanguage
                          && this.props.DeleteHTServiceLanguage(x.Id)}
                                    style={{background: 'transparent', border: 'transparent', boxShadow: 'none'}}
                          >
                            <i className="fa fa-remove"/>
                          </span>}
                      </a>
                    </li>)
                }
              </ul>
              <div className="tab-content">
                {
                  this.props.SelectedHTService.HTServiceLanguages.map(x => {
                    return <ServiceLanguageDetail
                      key={x.Id}
                      IsSelected={x.Language == this.props.SelectedLanguage}
                      HTServiceLanguage={x}
                      OnObjectChange={(obj: HTServiceLanguageModel) =>
                        this.props.OnHTServiceLanguageChange(obj)}
                    />
                  })
                }
              </div>
            </div>
            <hr/>
            <div className="form-group">
              <button className="btn btn-danger pull-right"
                      onClick={() => this.deleteHTService()}><i
                className="fa fa-trash-o"/> Xóa
              </button>

              <button className="btn btn-primary"
                      onClick={() => this.saveHTService()}>Lưu
              </button>

              <div className="col-sm-4">
                <ComboBox
                  placeHolder="Chọn category..."
                  options={Categories}
                  value={this.props.SelectedHTService.CategoryId}
                  onChange={(Id) => this.props.ClickSlectCategory(Id)}
                />
              </div>

              <div className="btn-group dropup mr10 ml10">
                <button type="button" className="btn btn-success dropdown-toggle"
                        data-toggle="dropdown" aria-expanded="false">
                  Thêm ngôn ngữ
                  <span className="caret"></span>
                </button>
                <ul className="dropdown-menu left animated fadeIn" role="menu">
                  {languages.filter(x =>
                    this.props.SelectedHTService && this.props.SelectedHTService.HTServiceLanguages &&
                    !this.props.SelectedHTService.HTServiceLanguages.some(r => r.Language == x.Language)
                  ).map((item, index) =>
                    <li key={index}>
                      <a onClick={() => this.props.AddHTServiceLanguage
                      && this.props.AddHTServiceLanguage(item.Language)}>{item.Title}</a>
                    </li>
                  )}
                </ul>
              </div>

              <button className="btn btn-default pull-right hidden"
                      onClick={() => this.discardChangesEditing()}>Làm lại
              </button>

            </div>
          </div> :
          <div className="col-lg-12 col-sm-12 form-horizontal">
            <div className="form-group">
              <span className="help-block">Click vào dịch vụ để xem thông tin chi tiết</span>
            </div>
          </div>
        }
      </div>
    )
  }

  private deleteHTService() {
    this.props.DeleteHTService && this.props.DeleteHTService(this.props.SelectedHTService.Id);
  }

  private discardChangesEditing() {

  }

  private saveHTService() {
    let isInvalid = false;
    let firstInvalid: HTServiceLanguageModel = null;
    this.props.SelectedHTService.HTServiceLanguages.forEach(x => {
      x['__#validated#__'] = true;
      if (x['__#isInvalid#__']) {
        firstInvalid = firstInvalid || x;
      }
      isInvalid = isInvalid || x['__#isInvalid#__'];
    });
    if (isInvalid) {
      this.props.ChangeSelectedLanguage(firstInvalid.Language);
      return;
    }
    // if is valid, do submit here
    console.log('congratulation! your form is valid, do submit now ' + this.props.SelectedHTService);
    this.props.SaveHTService && this.props.SaveHTService();
  }
}

export default HTServiceDetail;