import { HugeiconsIcon } from '@hugeicons/react';
import {
  UsersRoundIcon,
  UserSettings01Icon,
  Layers01Icon,
  File01Icon,
  HistoryIcon,
  Home03Icon as Home,
  Settings01Icon,
  Invoice01Icon,
  DashboardSquare01Icon,
  UserMultipleIcon,
  Shield01Icon,
  Mail01Icon,
  Activity01Icon,
  Notification01Icon,
  CreditCardIcon,
  Wallet01Icon,
  Ticket01Icon,
  Clock01Icon,
  Package01Icon,
} from '@hugeicons/core-free-icons';
import { DEFAULT_MENU_GROUP, navStore } from '@axiomframework/react-core';

const IDENTITY_MANAGEMENT = 'Identity Management';
const AUDIT = 'Audit';
const TENANT_MANAGEMENT = 'Tenant Management';

const PANEL_MANAGEMENT = 'Panel Management';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/',
    icon: <HugeiconsIcon icon={Home} strokeWidth={2} />,
  });

  navStore.addGroup(IDENTITY_MANAGEMENT);
  navStore.add(
    {
      title: 'Users',
      url: '/identity-management/users',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    IDENTITY_MANAGEMENT,
  );
  navStore.add(
    {
      title: 'Roles',
      url: '/identity-management/roles',
      icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} />,
    },
    IDENTITY_MANAGEMENT,
  );

  navStore.addGroup(TENANT_MANAGEMENT);
  navStore.add(
    {
      title: 'Tenants',
      url: '/tenant-management/tenants',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
    },
    TENANT_MANAGEMENT,
  );
  navStore.add(
    {
      title: 'Editions',
      url: '/tenant-management/editions',
      icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
    },
    TENANT_MANAGEMENT,
  );

  navStore.addGroup(PANEL_MANAGEMENT);
  navStore.add(
    {
      title: 'Panels',
      mode: 'switch-panel',
      icon: <HugeiconsIcon icon={Settings01Icon} strokeWidth={2} />,
      groups: [
        {
          title: DEFAULT_MENU_GROUP,
          children: [
            {
              title: 'Overview',
              url: '/panels/overview',
              icon: (
                <HugeiconsIcon icon={DashboardSquare01Icon} strokeWidth={2} />
              ),
            },
            {
              title: 'Activity',
              url: '/panels/activity',
              icon: <HugeiconsIcon icon={Activity01Icon} strokeWidth={2} />,
            },
            {
              title: 'Notifications',
              url: '/panels/notifications',
              icon: <HugeiconsIcon icon={Notification01Icon} strokeWidth={2} />,
            },
          ],
        },
        {
          title: 'Users Management',
          mode: 'collapsible',
          isActive: false,
          children: [
            {
              title: 'Overview',
              url: '/panels/users',
              icon: (
                <HugeiconsIcon icon={DashboardSquare01Icon} strokeWidth={2} />
              ),
            },
            {
              title: 'List',
              url: '/panels/users/list',
              icon: <HugeiconsIcon icon={UserMultipleIcon} strokeWidth={2} />,
            },
            {
              title: 'Roles',
              icon: <HugeiconsIcon icon={Shield01Icon} strokeWidth={2} />,
              children: [
                { title: 'Admin', url: '/panels/users/roles/admin' },
                { title: 'Editor', url: '/panels/users/roles/editor' },
                { title: 'Viewer', url: '/panels/users/roles/viewer' },
              ],
            },
            {
              title: 'Invitations',
              icon: <HugeiconsIcon icon={Mail01Icon} strokeWidth={2} />,
              children: [
                { title: 'Pending', url: '/panels/users/invitations/pending' },
                { title: 'Sent', url: '/panels/users/invitations/sent' },
                { title: 'Expired', url: '/panels/users/invitations/expired' },
              ],
            },
          ],
        },
        {
          title: 'Billing Management',
          children: [
            {
              title: 'Overview',
              url: '/panels/billing',
              icon: (
                <HugeiconsIcon icon={DashboardSquare01Icon} strokeWidth={2} />
              ),
            },
            {
              title: 'Billing',
              mode: 'switch-panel',
              icon: <HugeiconsIcon icon={Invoice01Icon} strokeWidth={2} />,
              groups: [
                {
                  title: DEFAULT_MENU_GROUP,
                  children: [
                    {
                      title: 'Invoices',
                      icon: (
                        <HugeiconsIcon icon={Invoice01Icon} strokeWidth={2} />
                      ),
                      children: [
                        {
                          title: 'Draft',
                          url: '/panels/billing/invoices/draft',
                        },
                        { title: 'Sent', url: '/panels/billing/invoices/sent' },
                        { title: 'Paid', url: '/panels/billing/invoices/paid' },
                        {
                          title: 'Overdue',
                          url: '/panels/billing/invoices/overdue',
                        },
                      ],
                    },
                    {
                      title: 'Payments',
                      icon: (
                        <HugeiconsIcon icon={CreditCardIcon} strokeWidth={2} />
                      ),
                      children: [
                        {
                          title: 'Succeeded',
                          url: '/panels/billing/payments/succeeded',
                        },
                        {
                          title: 'Pending',
                          url: '/panels/billing/payments/pending',
                        },
                        {
                          title: 'Failed',
                          url: '/panels/billing/payments/failed',
                        },
                      ],
                    },
                    {
                      title: 'Refunds',
                      icon: <HugeiconsIcon icon={Ticket01Icon} strokeWidth={2} />,
                      children: [
                        {
                          title: 'Requested',
                          url: '/panels/billing/refunds/requested',
                        },
                        {
                          title: 'Approved',
                          url: '/panels/billing/refunds/approved',
                        },
                        {
                          title: 'Rejected',
                          url: '/panels/billing/refunds/rejected',
                        },
                      ],
                    },
                  ],
                },
                {
                  title: 'Subscriptions',
                  children: [
                    {
                      title: 'Overview',
                      url: '/panels/billing/subscriptions',
                      icon: (
                        <HugeiconsIcon
                          icon={DashboardSquare01Icon}
                          strokeWidth={2}
                        />
                      ),
                    },
                    {
                      title: 'Subscription Settings',
                      mode: 'switch-panel',
                      icon: (
                        <HugeiconsIcon icon={Settings01Icon} strokeWidth={2} />
                      ),
                      groups: [
                        {
                          title: DEFAULT_MENU_GROUP,
                          children: [
                            {
                              title: 'Plans',
                              icon: (
                                <HugeiconsIcon
                                  icon={Package01Icon}
                                  strokeWidth={2}
                                />
                              ),
                              children: [
                                {
                                  title: 'Free',
                                  url: '/panels/billing/subscriptions/plans/free',
                                },
                                {
                                  title: 'Pro',
                                  url: '/panels/billing/subscriptions/plans/pro',
                                },
                                {
                                  title: 'Enterprise',
                                  url: '/panels/billing/subscriptions/plans/enterprise',
                                },
                              ],
                            },
                            {
                              title: 'Coupons',
                              icon: (
                                <HugeiconsIcon
                                  icon={Ticket01Icon}
                                  strokeWidth={2}
                                />
                              ),
                              children: [
                                {
                                  title: 'Active',
                                  url: '/panels/billing/subscriptions/coupons/active',
                                },
                                {
                                  title: 'Expired',
                                  url: '/panels/billing/subscriptions/coupons/expired',
                                },
                              ],
                            },
                            {
                              title: 'Trials',
                              icon: (
                                <HugeiconsIcon
                                  icon={Clock01Icon}
                                  strokeWidth={2}
                                />
                              ),
                              children: [
                                {
                                  title: 'Active',
                                  url: '/panels/billing/subscriptions/trials/active',
                                },
                                {
                                  title: 'Ended',
                                  url: '/panels/billing/subscriptions/trials/ended',
                                },
                              ],
                            },
                          ],
                        },
                        {
                          title: 'Payment Methods',
                          children: [
                            {
                              title: 'Cards',
                              icon: (
                                <HugeiconsIcon
                                  icon={CreditCardIcon}
                                  strokeWidth={2}
                                />
                              ),
                              children: [
                                {
                                  title: 'Credit',
                                  url: '/panels/billing/subscriptions/cards/credit',
                                },
                                {
                                  title: 'Debit',
                                  url: '/panels/billing/subscriptions/cards/debit',
                                },
                              ],
                            },
                            {
                              title: 'Wallets',
                              icon: (
                                <HugeiconsIcon
                                  icon={Wallet01Icon}
                                  strokeWidth={2}
                                />
                              ),
                              children: [
                                {
                                  title: 'Apple Pay',
                                  url: '/panels/billing/subscriptions/wallets/apple-pay',
                                },
                                {
                                  title: 'Google Pay',
                                  url: '/panels/billing/subscriptions/wallets/google-pay',
                                },
                                {
                                  title: 'PayPal',
                                  url: '/panels/billing/subscriptions/wallets/paypal',
                                },
                              ],
                            },
                          ],
                        },
                      ],
                    },
                  ],
                },
              ],
            },
          ],
        },
      ],
    },
    PANEL_MANAGEMENT,
  );

  navStore.addGroup(AUDIT);
  navStore.add(
    {
      title: 'Audit Logs',
      url: '/audit/logs',
      icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} />,
    },
    AUDIT,
  );
  navStore.add(
    {
      title: 'Entity Changes',
      url: '/audit/entity-changes',
      icon: <HugeiconsIcon icon={HistoryIcon} strokeWidth={2} />,
    },
    AUDIT,
  );
}
