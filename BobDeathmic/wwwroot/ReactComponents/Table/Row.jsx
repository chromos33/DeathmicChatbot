class Row extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        const components = {
            TextColumn: TextColumn
        }
        if (this.props.Columns.length > 0) {
            var i = 0;
            var curthis = this;
            const Columns = this.props.Columns.map((column) => {
                i++
                const ColumnType = components[column.ReactComponentName];
                return <ColumnType Sort={curthis.props.Sort} key={i} id={i} data={column} />;
            });
            return (<div>{Columns}</div>);
        }
        else {
            return <span>ERROR</span>;
        }
        

    }
}
