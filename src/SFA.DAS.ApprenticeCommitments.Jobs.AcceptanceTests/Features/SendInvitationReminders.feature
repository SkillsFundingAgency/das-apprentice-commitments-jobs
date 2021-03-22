Feature: SendInvitationReminders
	I want to start the process of sending invitation reminders 

Scenario: Each day, just after midnight I want to start sending reminder emails
	Given the reminder period is set to be 9 days
	When the scheduled job starts
	Then a request to send reminder emails is sent with 9 days

Scenario: Each day, just after midnight I want to start sending reminder emails with a default value
	Given the reminder is not set
	When the scheduled job starts
	Then a request to send reminder emails is sent with 7 days
