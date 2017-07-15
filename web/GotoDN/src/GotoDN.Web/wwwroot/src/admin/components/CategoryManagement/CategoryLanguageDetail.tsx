import * as React from 'react';
import {CategoryLanguageModel} from "../../../models/CategoryLanguageModel";
import {DynamicPanelComponent} from "../DynamicForm/DynamicPanelComponent";
import {DynamicFormModel} from "../../../models/dynamic-form-model";
import {DynamicFieldModel} from "../../../models/dynamic-field-model";
import {FieldStructureTypeEnums} from "../../../models/field-structure-model";
import {ValidateRuleTypeEnums} from "../../../commons/constant";
interface thisProps {
  CategoryLanguage: CategoryLanguageModel,
  IsSelected: boolean,
  OnObjectChange: (obj: CategoryLanguageModel) => void
}

interface thisState {
  EditingObjec1t?: CategoryLanguageModel
}

class CategoryLanguageDetail extends React.Component<thisProps, thisState> {
  editingForm: DynamicPanelComponent;
  state: thisState = {};

  setState(state: thisState) {
    super.setState(state);
  }

  render() {
    return (
      <div className={`tab-pane fade${this.props.IsSelected ? ' active in' : ''}`}>
        {this.props.CategoryLanguage ?
          <DynamicPanelComponent
            ref={(r) => this.editingForm = r}
            FormStructure={this.getFormStructure()}
            onFieldValueChange={(obj) => {
              this.props.OnObjectChange(obj)
            }}
            Object={this.props.CategoryLanguage}
            onValidationChange={(isInvalid) => {
              this.props.CategoryLanguage['__#isInvalid#__'] = isInvalid
            }}
          /> : null}
      </div>);
  }

  private getFormStructure(): DynamicFormModel[] {
    let allForms: DynamicFormModel[] = [];
    {
      let inforForm: DynamicFormModel = {
        Icon: 'fa fa-info',
        Priority: 1,
        Title: '',
        BlankPanel: true,
        DynamicFields: []
      };
      let f_Title: DynamicFieldModel = {
        Priority: 1,
        LabelClass: 'col-lg-3',
        InputClass: 'col-lg-9',
        FieldStructure: {
          Name: 'Tiêu đề',
          FieldName: 'Title',
          PlaceHolder: '',
          Type: FieldStructureTypeEnums.TextBox,
          ValidateRules: [{
            Type: ValidateRuleTypeEnums.Required,
            InValidMessage: 'Trường này là bắt buộc'
          },
            {
              Type: ValidateRuleTypeEnums.MinLength,
              InValidMessage: 'Yêu cầu ít nhất 3 ký tự',
              RuleData: '3'
            },
            {
              Type: ValidateRuleTypeEnums.MaxLength,
              InValidMessage: 'Không được vượt quá 50 ký tự',
              RuleData: '50'
            }
          ]
        }
      };
      inforForm.DynamicFields.push(f_Title);
      allForms.push(inforForm);
    }
    return allForms;
  }

  private saveCategory() {
    if (this.editingForm.isFormValid()) {

    }
    this.forceUpdate();
  }
}

export default CategoryLanguageDetail;