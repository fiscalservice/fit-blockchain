import * as React from 'react';
import { withRouter } from 'react-router-dom';
import { withStyles } from 'material-ui/styles';
import Button from 'material-ui/Button';
import Card, { CardHeader, CardContent } from 'material-ui/Card';
import Input from 'material-ui/Input';
import Typography from 'material-ui/Typography';
import 'moment-timezone';
import Moment from 'react-moment';
import 'momentjs';


import styles from './LoginPage.styles';
import users from './users';
import AppHeader from '../../shared/AppHeader/AppHeader';
import LayoutGrid from 'material-ui/Grid';
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
import {
    BrowserRouter as Router,
    Route,
    Link
  } from 'react-router-dom'
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

  
  class searchPage extends React.Component {
   
   
    state = { assetID: '5000', deviceID: '', failedLogin: false}; 
   
    componentDidMount() {
        const getLastItem = (arr, predicate) => arr.filter(predicate).slice(-1)[0];
               
        const getStatus = (lastLogin) => {
          const now = Date.now();
          const tenDays = 1000 * 60 * 60 * 24 * 10;
          if (lastLogin && (now - lastLogin.returnValues.time) < tenDays) {
            return 'Responded';
          }
          return 'Inactive';
        };
        const handleResponse = (res) => (res && res.ok) ? res.json() : Promise.reject(res);
        
        
        let url = '/api/transactions' ;


        Promise.all([
          fetch(url).then(handleResponse)
        ]).then(([inventory]) => {
            const rows = inventory.map(phone => {
            const event = phone.event; 
            const userId = phone.userId;   
            const assetId = phone.assetId;
            const timestamp = phone.timeStamp;
            var actualTime= moment(timestamp).format('MMMM Do YYYY, h:mm:ss a');
            const status = phone.status;
           
    
            
            return {
              event: event,
              userID: userId,
              //userID: lastLogin ? lastLogin.returnValues.user : undefined,
              assetID: assetId,
              userID: userId,
              status: status,
              timestamp: actualTime
              //timestamp: theTime,
              
            }
          });
          this.setState({ rows });
        });
      }

      updateState = ({ target }) => {
        this.setState({
            [target.name]: target.value,
            failedLogin: false
        });
    }
          
      constructor(props) {
        super(props);
        this.state = {value: '5000'};
        this.state = {
          columns: [
            { name: 'event', title: 'Event' },
            { name: 'userID', title: 'UserID' },
            { name: 'assetID', title: 'AssetID' },
            { name: 'status', title: 'Status' },
            { name: 'timestamp', title: 'TimeStamp' },     
          ],
          rows: [
            
          ]
        };
      }
    
    render() {
    
        const { assetID, userID} = this.state;
        const { rows, columns} = this.state;
       
        return (
            <div >
           
                <AppHeader title="Transaction History"/>
        
             
                    
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
                    <ul>
            </ul>
            
            </div>
        );
    }
}

export default withRouter(withStyles(styles)(searchPage));