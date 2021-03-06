import { sidebarDisplayContext } from './context';
import { CopyTextButton } from './CopyTextButton';
import { DisplayMarkdownButton } from './DisplayMarkdownButton';
import { DownloadTextButton } from './DownloadTextButton';
import { SidebarButton } from './SidebarButton';
import { SidebarTools } from './SidebarTools';

const Buttons = {
	CopyText: CopyTextButton,
	DownloadText: DownloadTextButton,
	DisplayMdx: DisplayMarkdownButton,
};

export const Sidebar = Object.assign(SidebarTools, {
	Button: SidebarButton,
	Buttons,
	Display: sidebarDisplayContext.Provider,
});
