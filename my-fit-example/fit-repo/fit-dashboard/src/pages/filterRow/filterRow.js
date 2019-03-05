import React from 'react';
import AppHeader from '../../shared/AppHeader/AppHeader';
import LayoutGrid from 'material-ui/Grid';

import 'moment-timezone';
import Moment from 'react-moment';
import 'momentjs';

import { getPhoneStatus } from '../../shared/api/data';



import {
  FilteringState,
  IntegratedFiltering,
} from '@devexpress/dx-react-grid';
import {
  Grid,
  Table,
  TableHeaderRow,
  TableFilterRow,
} from '@devexpress/dx-react-grid-material-ui';
import Paper from 'material-ui/Paper';
var moment = require('moment');



const HeaderCell = ({ title, style }) => (
  <th style={{
    textAlign: 'left',
    fontSize: '1.5em',
    fontWeight: 'bold',
    paddingLeft: '1rem',
    ...style
  }}>
    {title}
  </th>
);

export default class Demo extends React.PureComponent {
  state = {activeAccounts: 0, accounts: []};
  
  
  componentDidMount() {
    
    const getLastItem = (arr, predicate) => arr.filter(predicate).slice(-1)[0];
    const handleResponse = (res) => (res && res.ok) ? res.json() : Promise.reject(res);
    Promise.all([
      fetch('/api/pastIngestions').then(handleResponse),
      fetch('/api/pastLogins').then(handleResponse),
     
      fetch('/api/deviceInventory').then(handleResponse)
    ]).then(([ingestions, logins, inventory]) => {
        const rows = inventory.map(phone => {
        const costCode = phone.toCostCodeType;
        const assetId = phone.assetId;
        const userId = phone.userId;
        const status = phone.status
        const timestamp = phone.timeStamp;
        var actualTime= moment(timestamp).format('MMMM Do YYYY, h:mm:ss a');
        var amountOfTime =moment(actualTime, "MMMM Do YYYY, h:mm:ss a").fromNow();
        var skype = getPhoneStatus(phone);
        
        //const lastTransfer = getLastItem(transfers, ({ assetId  }) => assetId === assetId);
        //const lastLogin = lastTransfer ? getLastItem(logins, ({ returnValues: { user } }) => user === lastTransfer.toUser) : undefined;

        return {
          costCode: costCode,
          assetId: assetId,
          //userID: lastLogin ? lastLogin.returnValues.user : undefined,
          userId: userId,
          
          status: status,

          
        

          //timestamp: lastLogin ? lastLogin.returnValues.time : undefined
     

          active: amountOfTime,

          skype: skype
          
        }
      });
      this.setState({ rows });
    });
  }
      
  constructor(props) {
    super(props);

    this.state = {
      columns: [
        { name: 'costCode', title: 'Cost Code' },
        { name: 'assetId', title: 'AssetID' },
        { name: 'userId', title: 'UserID' },
        { name: 'status', title: 'State' },
        
        { name: 'active', title: 'Latest Activity'} ,
        { name: 'skype', title: 'Status(30mins)'}  
      ],
      rows: [
        
      ]
    };
  }
  render() {
    const { rows, columns } = this.state;
    return (
      <div>
        <AppHeader title="Device Inventory" />
        <LayoutGrid container justify="center">
          <LayoutGrid item md={10} sm={12}>
            <Paper>
              <Grid
                rows={rows}
                columns={columns}
              >
                <FilteringState defaultFilters={[]} />
                <IntegratedFiltering />
                <Table />
                <TableHeaderRow cellComponent={({ column, style }) => <HeaderCell title={column.title} style={style} />} />
                <TableFilterRow />
              </Grid>
            </Paper>
          </LayoutGrid>
          
        </LayoutGrid>
      </div>
    );
  }
}
