class TemplateList extends React.Component {
    constructor(props) {
        super(props);
        this.state = { Templates: [] };
        this.handleAddTemplateClick = this.handleAddTemplateClick.bind(this);
        this.handleAddTemplateClick = this.handleAddTemplateClick.bind(this);
        this.handleUpdateTemplates = this.handleUpdateTemplates.bind(this);
    }
    componentWillMount() {
        var thisreference = this;
        $.ajax({
            url: "/Events/Templates/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                thisreference.setState({ Templates: result });
            }
        });
        this.props.eventEmitter.addListener("UpdateTemplates", thisreference.handleUpdateTemplates);
    }
    handleUpdateTemplates(event) {
        var tempthis = this;
        $.ajax({
            url: "/Events/Templates/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                tempthis.setState({ Templates: result });
            }
        });
    }
    handleAddTemplateClick(event) {
        tempthis = this;
        $.ajax({
            url: "/Events/AddTemplate/" + this.props.ID,
            type: "GET",
            data: {},
            success: function (result) {
                var temptemplates = tempthis.state.Templates;
                temptemplates.push(result);
                tempthis.setState({ Templates: temptemplates });
            }
        });
    }
    render() {
        if (this.state.Templates.length > 0) {
            var tempthis = this;
            templateNodes = this.state.Templates.map(function (template) {
                return <Template calendar={tempthis.props.ID} eventEmitter={tempthis.props.eventEmitter} key={template.key} name={template.key} day={template.Day} start={template.Start} stop={template.Stop} />;
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