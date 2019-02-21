import React from 'react';
import AppHeader from '../../shared/AppHeader/AppHeader';
import LayoutGrid from 'material-ui/Grid';

import 'moment-timezone';
import Moment from 'react-moment';
import 'momentjs';



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
  state = {activeAccounts: 0, accounts: [] };
  
  componentDidMount() {
    const getLastItem = (arr, predicate) => arr.filter(predicate).slice(-1)[0];
    const getStatus = (actualTime) => {
      const now = Date.now();
      const tenHours = 1000 * 60 * 60 * 24;
      if ((now - actualTime) < tenHours) {
        return 'Responded';
      }
      return 'Inactive';
    };
    const handleResponse = (res) => (res && res.ok) ? res.json() : Promise.reject(res);
    Promise.all([
      fetch('/api/pastIngestions').then(handleResponse),
      fetch('/api/pastLogins').then(handleResponse),
     
      fetch('/api/deviceInventory').then(handleResponse)
    ]).then(([ingestions, logins, inventory]) => {
        const rows = ingestions.map(phone => {
        const assetId = phone.assetId;
        const owner = phone.owner;
        const type = phone.typeOf;
        const time = phone.time;

        var actualTime= moment(time).format('MMMM Do YYYY, h:mm:ss a');
        var newTime = moment(time).format("YYYYMMDD");
        //var expire =moment(newTime, "YYYYMMDD").fromNow();
        var expire = moment(time).add(2, 'months').calendar(); 
        var expirationDate = moment(expire).endOf('day').fromNow();
       
        
        
        
        
        
        //const lastTransfer = getLastItem(transfers, ({ assetId  }) => assetId === assetId);
        //const lastLogin = lastTransfer ? getLastItem(logins, ({ returnValues: { user } }) => user === lastTransfer.toUser) : undefined;

        return {
         
          assetId: assetId,
          //userID: lastLogin ? lastLogin.returnValues.user : undefined,
          owner: owner,
          //status: getStatus(lastLogin),
          type: type,
          
          expire: expire,

          expirationDate: expirationDate

          
        

          //timestamp: lastLogin ? lastLogin.returnValues.time : undefined
         

          //active: getStatus(actualTime),
          
        }
      });
      this.setState({ rows });
    });
  }
      
  constructor(props) {
    super(props);

    this.state = {
      columns: [
      
        { name: 'assetId', title: 'AssetID' },
        { name: 'owner', title: 'Owner' },
        { name: 'type', title: 'Device Type' },

        
       
        { name: 'expire', title: 'Expiration Date' },
        
        { name: 'expirationDate', title: 'Set to Expire'}    
      ],
      rows: [
        
      ]
    };
  }
  render() {
    const { rows, columns } = this.state;
    return (
      <div>
        <AppHeader title="Devices Nearing End of Life" />
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
