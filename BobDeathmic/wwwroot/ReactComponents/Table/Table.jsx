class Table extends React.Component {
    constructor(props) {
        super(props);
        //Sort init,desc,asc
        this.state = { Rows: [], Filter: "" ,Sort: "init", SortColumn: 0};
        this.Search = this.Search.bind(this);
        this.Sort = this.Sort.bind(this);
        this.handleUpdateEvent = this.handleUpdateEvent.bind(this);
        
    }
    handleUpdateEvent(e) {
        this.UpdateData();
    }
    componentDidMount() {
        window.addEventListener('updateTable', this.handleUpdateEvent);
    }
    componentWillMount() {
        this.UpdateData();
    }
    UpdateData() {
        var thisreference = this;
        const xhr = new XMLHttpRequest();
        xhr.open('GET', thisreference.props.DataLink, true);
        xhr.onload = function () {
            thisreference.setState({ Table: JSON.parse(xhr.responseText) });
        };
        xhr.send();
    }
    Search(value) {
        this.setState({Filter: value});
    }
    Sort(field) {
        let sort = "";
        switch (this.state.Sort) {
            case "init":
                sort = "asc";
                break;
            case "desc":
                sort = "init";
                break;
            case "asc":
                sort = "desc";
                break;
        }
        this.setState({SortColumn: field,Sort: sort});
    }
    render() {
        if (this.state.Table !== undefined && this.state.Table.Rows.length > 0) {
            let i = 0;
            var curthis = this;
            var TableRows = this.state.Table.Rows;
            if (this.state.SortColumn !== 0 && this.state.Sort !== "init") {
                TableRows.sort(function (a, b) {
                    let index = curthis.state.SortColumn - 1;
                    //could solve this another way by filtering first row as header row but ...
                    if (a.isStatic) {
                        return 0;
                    }
                    else {
                        if (curthis.state.Sort === "desc") {
                            if (a.Columns[index].Text.toLowerCase() > b.Columns[index].Text.toLowerCase()) { return -1; }
                            if (a.Columns[index].Text.toLowerCase() < b.Columns[index].Text.toLowerCase()) { return 1; }
                        }
                        else {
                            if (a.Columns[index].Text.toLowerCase() < b.Columns[index].Text.toLowerCase()) { return -1; }
                            if (a.Columns[index].Text.toLowerCase() > b.Columns[index].Text.toLowerCase()) { return 1; }
                        }
                        return 0;
                    }
                });
            }
            const Rows = TableRows.map((row) => {
                i++;
                if (curthis.state.Filter !== "") {
                    if (!row.canFilter || row.canFilter && row.Filter.toLowerCase().includes(curthis.state.Filter.toLowerCase())) {
                        return <Row ColumnTypes={this.props.Columns} Sort={curthis.Sort} key={i} Columns={row.Columns} />;
                    }
                }
                else {
                    return <Row ColumnTypes={this.props.Columns} Sort={curthis.Sort} key={i} Columns={row.Columns} />;
                }
                
            });
            return (<div className="relative d-inline-block">
                <Search callback={this.Search} />
                <table>
                    <tbody>
                        {Rows}
                        </tbody>
                </table>
            </div>);
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
