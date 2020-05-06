class Table extends React.Component {
    constructor(props) {
        super(props);
        //Sort init,desc,asc
        this.state = { Rows: [], Filter: "" ,Sort: "init", SortColumn: 0};
        this.Search = this.Search.bind(this);
        this.Sort = this.Sort.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        const xhr = new XMLHttpRequest();
        xhr.open('GET', "/User/SubscriptionsData/", true);
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
        console.log(sort);
        this.setState({SortColumn: field,Sort: sort});
    }
    render() {
        if (this.state.Table !== undefined && this.state.Table.Rows.length > 0) {
            let i = 0;
            var curthis = this;
            var TableRows = this.state.Table.Rows;
            if (this.state.SortColumn !== 0 && this.state.Sort !== "init") {
                console.log(TableRows);
                TableRows.sort(function (a, b) {
                    let index = curthis.state.SortColumn - 1;
                    console.log(a);
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
                        return <Row Sort={curthis.Sort} key={i} Columns={row.Columns} />;
                    }
                }
                else {
                    return <Row Sort={curthis.Sort} key={i} Columns={row.Columns} />;
                }
                
            });
            return (<div>
                <Search callback={this.Search} />
                <div>
                    {Rows}
                </div>
            </div>);
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
