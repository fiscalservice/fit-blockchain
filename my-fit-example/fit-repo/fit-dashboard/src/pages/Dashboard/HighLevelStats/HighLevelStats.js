import * as React from 'react';
import { withStyles } from 'material-ui/styles';
import Card from 'material-ui/Card';
import Grid from 'material-ui/Grid';

const styles = (theme) => ({
    card: {
        padding: '30px',
        display: 'inline-flex',
         
    },
    greencard: {
        padding: '30px',
        display: 'inline-flex',
        backgroundColor: '#C8E6C9',
    }
});




const HighLevelStats = ({ data, classes }) => (
    <div>
        <Grid className={classes.card} container spacing={40}>
        <Card className={classes.card}>Total Devices: {data.total}</Card>
        
        <Card className={classes.card}>Active: {data.active}</Card>
        <Card className={classes.greencard}>Percent Active: {data.percent}</Card>
        </Grid>
    </div>
);

export default withStyles(styles)(HighLevelStats);
