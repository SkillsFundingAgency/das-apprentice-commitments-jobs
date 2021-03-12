Feature: NewApprenticeship
	When an apprenticeship is approved by employer and provider
	As a interested observer 
	I want to create that apprenticeship for the apprentice to confirm

Scenario: When an apprenticeship has been approved, create the apprenticeship for the apprentice to confirm 
	Given outer api is available
	When an apprenticeship is approved
	Then an apprenticeship record is created