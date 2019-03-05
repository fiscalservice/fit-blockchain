import * as React from 'react';
import AppHeader from '../../shared/AppHeader/AppHeader';
import HighLevelStats from './HighLevelStats/HighLevelStats';
import PhoneTable from './PhoneTable/PhoneTable';
import getData from '../../shared/api/data';
import Grid from 'material-ui/Grid';

class Dashboard extends React.Component {
    state = {
        data: { stats: {}, table: [] }
    };
    componentDidMount() {
        const { user } = this.props;
        getData(user).then(data => this.setState({ data }));
    }
    
    render() {
        const { data } = this.state;
        return (
            <div>
                <AppHeader title="Breakdown By Cost Code" />
                <Grid container justify="center">
                    <Grid item>
                        <HighLevelStats data={data.stats} />
                    </Grid>
                    <Grid item md={10} sm={12}>
                        <PhoneTable data={data.table} />
                    </Grid>
                </Grid>
            </div>
        );
    }
}

export default Dashboard;
