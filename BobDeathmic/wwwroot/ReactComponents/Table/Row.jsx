class Row extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        const components = {
            TextColumn: TextColumn
        }
        console.log(this.props);
        if (this.props.Columns.length > 0) {
            let i = 0;
            const Columns = this.props.Columns.map((column) => {
                i++
                const ColumnType = components[column.ReactComponentName];
                return <ColumnType key={i} data={column} />;
            });
            return (<div>{Columns}</div>);
        }
        else {
            return <span>ERROR</span>;
        }
        

    }
}
