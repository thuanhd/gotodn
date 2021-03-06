import * as React from 'react';
import {DistrictServiceInstance} from "../../services/DistrictService";
import {CityModel, DistrictModel} from "../../../models/CityModel";
import {GetGridRequestModel, GetGridResponseModel, ReactTable} from "../../../commons/react-table";
import {TableHeaderColumn} from 'react-bootstrap-table';
import DistrictModal from "../../components/DistrictManagement/DistrictModal";
import {CityServiceInstance} from "../../services/CityService";
import {SweetAlertResultEnums, SweetAlerts, SweetAlertTypeEnums} from "../../../commons/sweet-alerts";

interface thisState {
  GridFilter?: GetGridRequestModel,
  GridData?: GetGridResponseModel,
  SelectedDistrict?: DistrictModel,
  Cities?: CityModel[],
}
class DistrictManagement extends React.Component<{}, thisState> {
  DistrictModal: DistrictModal;
  state: thisState = {
    GridFilter: {
      CurrentPage: 1,
      IsAsc: false,
      SortExpression: "Id",
      PageSize: 10
    }
  };

  setState(state: thisState) {
    super.setState(state);
  }

  componentDidMount() {
    this.getData(this.state.GridFilter);
    (async () => {
      this.setState({
        Cities: await CityServiceInstance.GetAllCity()
      });
    })();
  }

  private async getData(request: GetGridRequestModel) {
    let result = await DistrictServiceInstance.Filter(request);
    if (result) {
      this.setState({GridData: result});
    }
  }

  private async createDistrict() {
    let result = await DistrictServiceInstance.CreateDistrict();
    if (result) {
      window['notice_create_success']();
      let filter = this.state.GridFilter;
      if (filter) {
        filter.CurrentPage = 1;
        filter.IsAsc = false;
        filter.SortExpression = "Id";
      }
      this.getData(filter);
      this.setState({
        SelectedDistrict: result,
      })
    }
    else {
      window['notice_error']();
    }
  }

  private async updateDistrict(model: DistrictModel) {
    let result = await DistrictServiceInstance.UpdateDistrict(model);
    if (result) {
      window['notice_save_success']();
      this.getData(this.state.GridFilter);
    }
    else {
      window['notice_error']();
    }
  }

  private async deleteDistrict(Id: number) {
    if (await SweetAlerts.show({
        type: SweetAlertTypeEnums.Warning,
        title: 'Xác nhận xóa',
        text: 'Bạn có chắc muốn xóa quận huyện này?',
        showCancelButton: true,
        confirmButtonText: 'Đồng ý xóa',
        closeOnConfirm: true
      }) == SweetAlertResultEnums.Confirm) {
      let result = await DistrictServiceInstance.DeleteDistrict(Id);
      if (result) {
        window['notice_delete_success']();
        let filter = this.state.GridFilter;
        if (filter) {
          filter.CurrentPage = this.state.GridData
          && this.state.GridData.DataSource
          && this.state.GridData.DataSource.length <= 1
            ? Math.max(filter.CurrentPage - 1, 1) : filter.CurrentPage
          ;
        }
        this.getData(filter);

        this.setState({
          SelectedDistrict: null,
        });
      }
      else {
        window['notice']('error-notice', 'Lỗi', 'Không thể xóa được bản ghi vì bản ghi được sử dụng trong hệ thống, bạn chỉ có thể xóa được bản ghi nếu nó không được sử dụng trong hệ thống.', 'glyphicon glyphicon-remove');
      }
    }
  }

  render() {
    return (
      <div className="page-content-wrapper">
        <div className={`page-content-inner`}>
          <div id="page-header" className="clearfix">
            <div className="page-header">
              <h2>Quận huyện</h2>
              <span className="txt">Quản lý quận huyện</span>
            </div>
            <div className="header-stats">
            </div>
          </div>

          <div className="row">
            <div className="col-lg-12 col-md-12 col-sm-12">
              <div className="panel panel-default plain toggle panelMove  ">
                <div className="panel-body">

                  <form className="form-inline">

                        <span className="input-group-btn">
                            <button className="btn btn-primary" type="button"
                                    onClick={() => this.createDistrict()}
                            ><i className="fa fa-plus"></i> Thêm quận huyện</button>
                          </span>
                  </form>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-lg-12 col-md-12 col-sm-12">
              <div className="panel panel-default plain toggle panelMove">
                <div className="panel-body">
                  <ReactTable request={this.state.GridFilter}
                              data={this.state.GridData}
                              trClassName={(d) => {
                                return ""
                              }}
                              defaultSortName={"Id"}
                              defaultSortOrder={false}
                              onFilterRequest={
                                (request) => {
                                  this.getData(request);
                                  this.setState({GridFilter: request});
                                }}>
                    <TableHeaderColumn width="100" dataField="Name"
                                       dataFormat={(r, data) => this.bindNametData(data)} dataSort={true}>
                      Tên quận huyện</TableHeaderColumn>
                    <TableHeaderColumn width="200" dataField="City" dataAlign="center"
                                       dataFormat={(r, data) => this.bindDistrictData(data)} dataSort={true}>
                      Thuộc tỉnh thành</TableHeaderColumn>
                    <TableHeaderColumn width="80" dataField="Action" dataAlign="center"
                                       dataFormat={(r, data) => this.bindActionData(data)} dataSort={ false }>
                      Thao tác</TableHeaderColumn>
                  </ReactTable>
                </div>
              </div>
            </div>
          </div>
        </div>
        <DistrictModal ref={e => this.DistrictModal = e}
                       SelectedDistrict={this.state.SelectedDistrict}
                       SaveDistrict={(model) => this.updateDistrict(model)}
                       OnChange={(model) => this.setState({SelectedDistrict: model})}
                       deleteDistrict={(Id: number) => this.deleteDistrict(Id)}
                       Cities={this.state.Cities}
                       ClickSlectCity={(Id: number) => this.clickSelectCity(Id)}
        />
      </div>
    );
  }

  private bindNametData(data: DistrictModel) {
    return <a onClick={() => {
      this.setState({SelectedDistrict: data});
      this.DistrictModal.show();
    }}>{data.Name || 'Chưa đặt tên'}</a>;
  }

  private bindDistrictData(data: DistrictModel) {
    let name = data && data.City && data.City.Name;
    return <span>{name}</span>;
  }

  private clickSelectCity(Id: number) {
    this.state.SelectedDistrict.CityId = Id;
    this.forceUpdate();
  }

  private bindActionData(data: any) {
    return (<div className="table--actions-container">
      <button className="btn btn-danger" onClick={() => this.deleteDistrict(data.Id)}>
        <i className="fa fa-trash" aria-hidden="true">Xóa</i>
      </button>
    </div>);
  }
}


export default DistrictManagement;