export interface AccountsCredentials {
	username?: string;
	password?: string;
	code?: number;
}

export interface ImageFile {
	filename?: string;
}

export interface Gamespace {
	id?: number;
	name?: string;
	whenCreated?: string;
	topologyDocument?: string;
	topologyId?: number;
}

export interface GameState {
	id?: number;
	name?: string;
	globalId?: string;
	whenCreated?: string;
	topologyDocument?: string;
	shareCode?: string;
	vms?: Array<VmState>;
}

export interface VmState {
	id?: string;
	templateId?: number;
	name?: string;
	isRunning?: boolean;
}

export interface ProfileSearchResult {
	search?: Search;
	total?: number;
	results?: Array<Profile>;
}

export interface Search {
	term?: string;
	skip?: number;
	take?: number;
	sort?: number;
	filters?: Array<string>;
}

export interface Profile {
	id?: number;
	globalId?: string;
	name?: string;
	isAdmin?: boolean;
}

export interface TemplateSummarySearchResult {
	search?: Search;
	total?: number;
	results?: Array<TemplateSummary>;
}

export interface TemplateSummary {
	id?: number;
	name?: string;
	topologyId?: number;
	topologyName?: string;
	parentId?: string;
	parentName?: string;
}

export interface Template {
	id?: number;
	parentId?: number;
	canEdit?: boolean;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
	isHidden?: boolean;
	topologyId?: number;
	topologyGlobalId?: string;
}

export interface TemplateDetail {
	id?: number;
	name?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface ChangedTemplate {
	id?: number;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
	isHidden?: boolean;
	topologyId?: number;
}

export interface TopologySummarySearchResult {
	search?: Search;
	total?: number;
	results?: Array<TopologySummary>;
}

export interface TopologySummary {
	id?: number;
	name?: string;
	description?: string;
	canManage?: boolean;
	canEdit?: boolean;
	isPublished?: boolean;
	author?: string;
}

export interface NewTopology {
	name?: string;
	description?: string;
}

export interface Topology {
	id?: number;
	globalId?: string;
	name?: string;
	description?: string;
	documentUrl?: string;
	shareCode?: string;
	canManage?: boolean;
	canEdit?: boolean;
	templateLimit?: number;
	isPublished?: boolean;
	workers?: Array<Worker>;
	templates?: Array<Template>;
}

export interface Worker {
	id?: number;
	personName?: string;
	canManage?: boolean;
	canEdit?: boolean;
}

export interface ChangedTopology {
	id?: number;
	name?: string;
	description?: string;
}

export interface TopologyState {
	id?: number;
	shareCode?: string;
	isPublished?: boolean;
}

export interface VmOptions {
	iso?: Array<string>;
	net?: Array<string>;
}

export interface VirtualVm {
	id?: string;
	name?: string;
	host?: string;
	path?: string;
	reference?: string;
	diskPath?: string;
	stats?: string;
	status?: string;
	state?: VirtualVmStateEnum;
	question?: VirtualVmQuestion;
	task?: VirtualVmTask;
}

export interface VirtualVmQuestion {
	id?: string;
	prompt?: string;
	defaultChoice?: string;
	choices?: Array<VirtualVmQuestionChoice>;
}

export interface VirtualVmTask {
	id?: string;
	name?: string;
	progress?: number;
	whenCreated?: string;
}

export interface VirtualVmQuestionChoice {
	key?: string;
	label?: string;
}

export interface KeyValuePair {
	id?: number;
	key?: string;
	value?: string;
}

export interface VirtualVmAnswer {
	questionId?: string;
	choiceKey?: string;
}

export enum VirtualVmStateEnum {
	off = <any>'off',
	running = <any>'running',
	suspended = <any>'suspended'
}
