class Table extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Rows: []};
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
    render() {
        if (this.state.Table !== undefined && this.state.Table.Rows.length > 0) {
            let i = 0;
            const Rows = this.state.Table.Rows.map((row) => {
                i++;
                return <Row key={i} Columns={row.Columns} />;
            });
            return (<div>
                {Rows}
            </div>);
        }
        else {
            return <span>Loading</span>;
        }
        
    }
}
