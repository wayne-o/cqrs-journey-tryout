cqrs-journey-tryout
===================


This is a port of the CQRS Journey code that came with the MS Patterns & Practices guidance on CQRS/ES.

I don't like Unity so I have swapped in Castle Windsor IOC. I also don't like the Settings.Xml file and have made it so the command and event handlers are autowired.

I didn't have time to complete this with the Azure ServiceBus config and code - ATM it only uses the SQL implementation.

This spike is part of my assesment of 2 CQRS/ES frameworks (this and GetEventStore) in respect of developing a "conversations" 
platform for sonatribe.com. 

There are 2 parts to this:

- The main API - if you use Postman Rest client then import this category:
                         https://www.getpostman.com/collections/4b5eae175a3bfb48459d
                 There is a GET and a POST - the post simply pushes a pretend conversation into the CQRS pipeline by
                 publishing a new CreateNewConversation command
                 
- The worker process - this handles the events and commands

The worker process has 2 parts

- The Domain handler  - this is the part that is responsible for ensuring domain invariants etc
- The Command handler - this is the part that is responsible for updating the read models

The read model is configured to use RavenDb because it is fast with reads and ridiculously easy to code with.
                 
This is just me messing about trying to find my feet with the CQRS Journey code - there's some stuff that shouldn't be where it is and I'm sure there could be further investigation into some of the bits I skipped over (or completely missed!)
