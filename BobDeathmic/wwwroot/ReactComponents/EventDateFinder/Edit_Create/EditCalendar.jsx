class EditCalendar extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: [] };
    }
    componentWillMount() {

        const xhr = new XMLHttpRequest();
        xhr.open('get', "/EventDateFinder/GetCalendar/" + this.props.ID , true);
        xhr.onload = () => {
            const data = JSON.parse(xhr.responseText);
            this.setState({ data: data });
        };
        xhr.send();
    }
    render() {
        if (this.state.data.name === undefined) {
            return (
                <div className="OverView">
                    <NameField owner={this.props.ID} value="" />
                </div>
            );
        }
        else {
            return (
                <div className="OverView">
                    <NameField owner={this.props.ID} value={this.state.data.name} />
                </div>
            );
        }
        
    }
}
