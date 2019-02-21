class TemplateList extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Templates: [] };
        this.handleAddTemplateClick = this.handleAddTemplateClick.bind(this);
        this.handleAddTemplateClick = this.handleAddTemplateClick.bind(this);
        this.handleRadioChange = this.handleRadioChange.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/EventDateFinder/Templates/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ Templates: result });
            }
        });
    }
    handleOnClick(event) {

    }
    handleOnChange(event) {

    }
    handleRadioChange(event) {
        alert("test");
    }
    handleAddTemplateClick(event) {
        $.ajax({
            url: "/EventDateFinder/AddTemplate/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ Templates: this.state.Templates.push(result) });
            }
        });
    }
    render() {
        if (this.state.Templates.length > 0) {
            templateNodes = this.state.Templates.map(function (template) {
                return <Template key={template.key} name={template.key} day={template.Day} start={template.Start} stop={template.Stop} />;
            });
            return (
                <div>
                    <div className="row" key={this.props.key}>
                        <div className="col-12">
                            <span onClick={this.handleAddTemplateClick} className="button">Add Template</span>
                        </div>
                        <div className="col-12">
                            {templateNodes}
                        </div>
                    </div>
                </div>
            );
        }
        return (<div className="row">
            <div className="col-12">
                <span onClick={this.handleAddTemplateClick} className="button">Add Template</span>
            </div>
        </div>);

    }
}