import { sidebarDisplayContext } from './context';
import { SidebarButton } from './SidebarButton';
import { SidebarTools } from './SidebarTools';

export const Sidebar = Object.assign(SidebarTools, { Button: SidebarButton, Display: sidebarDisplayContext.Provider });
