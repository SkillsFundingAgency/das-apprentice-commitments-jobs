Feature: SendInvitationReminders
	I want to start the process of sending invitation reminders 

Scenario: Each day, just after midnight I want to start sending reminder emails
	Given the reminder period is set
	When the scheduled job starts
	Then a request to send reminder emails is sent with expected value