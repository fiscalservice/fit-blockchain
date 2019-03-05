import * as React from 'react';
import { withStyles } from 'material-ui/styles';
import Table, { TableBody, TableCell, TableHead, TableRow } from 'material-ui/Table';
import Paper from 'material-ui/Paper/Paper';

const styles = (theme) => ({
    headerCell: {
        fontSize: '1.5em',
        fontWeight: 'bold'
    }
});

const PhoneTable = ({ data, classes }) => (
    <Paper>
        <Table>
            <TableHead>
                <TableRow>
                    <TableCell className={classes.headerCell}>Cost Code</TableCell>
                    <TableCell className={classes.headerCell} numeric>Total Devices</TableCell>
                    <TableCell className={classes.headerCell} numeric>Active</TableCell>
                    <TableCell className={classes.headerCell} numeric>Percent Active</TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
                {data.map((row, index) => (
                    <TableRow key={row.costCode}>
                        <TableCell>{row.costCode}</TableCell>
                        <TableCell numeric>{row.total}</TableCell>
                        <TableCell numeric>{row.active}</TableCell>
                        <TableCell numeric>{row.percent}</TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    </Paper>
);

export default withStyles(styles)(PhoneTable);
