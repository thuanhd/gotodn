import React from 'react';
import {View, Image, TouchableOpacity, ScrollView} from 'react-native';
import {Col, Row, Grid} from 'react-native-easy-grid';

import {MenuListItemData} from '../common/constain';
import {Icon} from 'native-base';
import {DetailBanner} from '../components/detail/DetailBanner';
import {style} from '../styles/style';
import {DetailText} from '../components/detail/DetailText';
import {DetailInfo} from '../components/detail/DetailInfo';
import {DetailImage} from '../components/detail/DetailImage';

export class DetailScreen extends React.Component {
  state = {
    dataDetail: {}
  };

  componentWillMount() {
  }

  componentDidMount() {
    const {params} = this.props.navigation.state;
    let itemId = (params && params.itemId) || 0;
    let data = MenuListItemData.filter(t => t.id == itemId);
    this.setState({
      dataDetail: data.length > 0 ? data[0] : null
    });
  }

  render() {

    let data = this.state.dataDetail;
    let detailInfo = [];
    detailInfo.push({infoIcon: data.addressIcon || 'ios-sad-outline', infoText: data.address});
    detailInfo.push({infoIcon: data.phoeneIcon || 'ios-sad-outline', infoText: data.phone});
    detailInfo.push({infoIcon: data.websiteIcon || 'ios-sad-outline', infoText: data.website,isUrl: true});
    detailInfo.push({infoIcon: data.openHourIcon || 'ios-sad-outline', infoText: data.openHour});

    return (
      data ? (
          <Grid>
            <Col>
              <ScrollView>
                <Row size={1}>
                  <DetailBanner
                    coverImg={data.heroImage}
                    onSharedClicked={() => this.shareDetail(data.id)}
                    onFavoriteClicked={() => this.likeDetail(data.id)}/>
                </Row>
                <Row size={2}>
                  <View style={style.detailContent}>
                    <DetailText title={data.title} description={data.description}/>
                    <DetailInfo detailInfo={detailInfo} />
                    <DetailImage images={data.images} />
                  </View>
                  {/*<HomeMenuList navigation={this.props.navigation}/>*/}
                </Row>
              </ScrollView>
            </Col>
          </Grid>) : null
    )
  }

  shareDetail(id) {

  }

  likeDetail(id) {

  }
}