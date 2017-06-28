import React from 'react';
import {} from 'react-native';
import {Col, Row, Grid} from 'react-native-easy-grid';
import TopSlider from '../slider/TopSlider';
import {SlideType} from '../../common/constain';
import {ListItemDetail} from './ListItemDetail';

export class ListDetail extends React.Component {
  render() {
    return (
      <Grid>
        <Row size={46}>
          <TopSlider sliderType={SlideType.ListScreen} title="SỰ KIỆN NỔI BẬT"/>
        </Row>
        <Row size={54}>
          <ListItemDetail navigation={this.props.navigation} />
        </Row>
      </Grid>

    );
  }
}